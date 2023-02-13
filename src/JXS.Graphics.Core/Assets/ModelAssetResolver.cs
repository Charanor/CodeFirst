using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Assimp;
using JXS.AssetManagement;
using JXS.FileSystem;
using JXS.Utils;
using Microsoft.VisualBasic.FileIO;
using OpenTK.Mathematics;

namespace JXS.Graphics.Core.Assets;

public class ModelAssetResolver : IAssetResolver
{
	private const int INDEX_SIZE = sizeof(int) + sizeof(char); // We add sizeof(char) because of the value separator

	private const int FLOATS_PER_VERTEX = 3 + 3 + 2; // Position + normal + texCoord
	private const int VERTEX_SIZE = sizeof(float) * FLOATS_PER_VERTEX + sizeof(char);

	private const string PACKAGED_MODEL_EXTENSION = ".csm";
	private const char VALUE_SEPARATOR = ',';

	private readonly Func<Material> materialProvider;

	public ModelAssetResolver(Func<Material> materialProvider)
	{
		this.materialProvider = materialProvider;
	}

	public bool CanLoadAssetOfType(Type type) => type == typeof(Model);

	public bool TryLoadAsset(FileHandle modelFile, [NotNullWhen(true)] out object? asset)
	{
		if (modelFile.HasExtension(PACKAGED_MODEL_EXTENSION))
		{
			return TryLoadPackagedModel(modelFile, out asset);
		}

		using var assimp = new AssimpContext();
		var formats = assimp.GetSupportedImportFormats();
		if (!formats.Contains(modelFile.Extension))
		{
			// We can't load a model of this format
			asset = default;
			return false;
		}

		var packagedModelFile = modelFile.SiblingWithDifferentExtension(PACKAGED_MODEL_EXTENSION);
		if (!packagedModelFile.Exists)
		{
			if (!GeneratePackagedModelFileV2(assimp, modelFile, packagedModelFile))
			{
				asset = default;
				return false;
			}
		}
#if DEBUG
		else
		{
			// If we are debugging, just recreate the file each time we run
			if (!GeneratePackagedModelFileV2(assimp, modelFile, packagedModelFile))
			{
				// Normally we would return false, here, but if we are debugging we probably want to know that re-creating the file failed.
				throw new Exception($"Failed to re-create packaged model file at {packagedModelFile}");
			}
		}
#endif

		// ReSharper disable once InvertIf
		if (packagedModelFile.Type != FileType.File)
		{
			asset = default;
			return false;
		}

		return TryLoadPackagedModelV2(packagedModelFile, out asset);
	}

	private bool TryLoadPackagedModel(FileHandle packagedModelFile, [NotNullWhen(true)] out object? model)
	{
		using var parser = new TextFieldParser(packagedModelFile.Read())
		{
			TextFieldType = FieldType.Delimited,
			Delimiters = new[] { VALUE_SEPARATOR.ToString() },
			TrimWhiteSpace = true
		};

		var meshes = new List<Mesh>();
		var currentIndices = new List<uint>();
		var currentVertices = new List<Mesh.Vertex>();
		while (!parser.EndOfData)
		{
			currentIndices.Clear();
			currentVertices.Clear();

			var meshData = parser.ReadFields();
			if (meshData == null)
			{
				continue;
			}

			var dataIdx = 0;
			var meshIndexCount = ReadInt(ref dataIdx);
			var meshVertexCount = ReadInt(ref dataIdx);
			for (var i = 0; i < meshIndexCount; i++)
			{
				currentIndices.Add(ReadInt(ref dataIdx));
			}

			for (var i = 0; i < meshVertexCount; i++)
			{
				var position = new Vector3(ReadFloat(ref dataIdx), ReadFloat(ref dataIdx), ReadFloat(ref dataIdx));
				var normal = new Vector3(ReadFloat(ref dataIdx), ReadFloat(ref dataIdx), ReadFloat(ref dataIdx));
				var texCoord = new Vector2(ReadFloat(ref dataIdx), ReadFloat(ref dataIdx));
				currentVertices.Add(new Mesh.Vertex(position, normal, texCoord));
			}

			meshes.Add(MainThread.Post(() => new Mesh(currentVertices, currentIndices, materialProvider())).Result);

			uint ReadInt(ref int i) => uint.Parse(meshData[i++], CultureInfo.InvariantCulture);
			float ReadFloat(ref int i) => float.Parse(meshData[i++], CultureInfo.InvariantCulture);
		}

		model = new Model(meshes);
		return true;
	}

	private static bool GeneratePackagedModelFile(AssimpContext assimp, FileHandle modelFile,
		FileHandle packagedModelFile)
	{
		var scene = assimp.ImportFile(modelFile.FilePath, PostProcessSteps.Triangulate);
		var meshes = new List<PackagedMeshIntermediateFormat>();
		foreach (var sceneMesh in scene.Meshes)
		{
			var indices = sceneMesh.GetIndices();
			var vertices = new Mesh.Vertex[sceneMesh.VertexCount];
			for (var i = 0; i < sceneMesh.VertexCount; i++)
			{
				vertices[i] = CreateForIndex(i);
			}

			meshes.Add(new PackagedMeshIntermediateFormat(indices, vertices));

			Mesh.Vertex CreateForIndex(int index) => new(
				ToOpenTK(sceneMesh.Vertices[index]),
				ToOpenTK(sceneMesh.Normals[index]),
				ToOpenTK(sceneMesh.TextureCoordinateChannels[0][index]).Xy
			);
		}

		var estimatedSize = 0;
		estimatedSize += INDEX_SIZE * meshes.Count; // The index count element 
		estimatedSize += INDEX_SIZE * meshes.Count; // The vertex count element 
		foreach (var (indices, vertices) in meshes)
		{
			estimatedSize += INDEX_SIZE * indices.Length;
			estimatedSize += VERTEX_SIZE * vertices.Length;
		}

		var output = new StringBuilder(estimatedSize);
		foreach (var (indices, vertices) in meshes)
		{
			Int(indices.Length);
			Int(vertices.Length);
			foreach (var index in indices)
			{
				Int(index);
			}

			foreach (var ((pX, pY, pZ), (nX, nY, nZ), (u, v)) in vertices)
			{
				Float(pX);
				Float(pY);
				Float(pZ);
				Float(nX);
				Float(nY);
				Float(nZ);
				Float(u);
				Float(v);
			}

			void Int(int value)
			{
				output.Append(value.ToString(CultureInfo.InvariantCulture));
				output.Append(VALUE_SEPARATOR);
			}

			void Float(float value)
			{
				output.Append(value.ToString(CultureInfo.InvariantCulture));
				output.Append(VALUE_SEPARATOR);
			}
		}

		var str = output.ToString()[..(output.Length - 1)];
		return packagedModelFile.Create(FileType.File) && packagedModelFile.WriteAllText(str);
	}

	private static bool GeneratePackagedModelFileV2(AssimpContext assimp, FileHandle modelFile,
		FileHandle packagedModelFile)
	{
		var scene = assimp.ImportFile(modelFile.FilePath, PostProcessSteps.Triangulate);
		var meshes = new List<PackagedMeshIntermediateFormat>();
		foreach (var sceneMesh in scene.Meshes)
		{
			var indices = sceneMesh.GetIndices();
			var vertices = new Mesh.Vertex[sceneMesh.VertexCount];
			for (var i = 0; i < sceneMesh.VertexCount; i++)
			{
				vertices[i] = CreateForIndex(i);
			}

			meshes.Add(new PackagedMeshIntermediateFormat(indices, vertices));

			Mesh.Vertex CreateForIndex(int index)
			{
				var texCoordChannel = sceneMesh.TextureCoordinateChannels[0];
				var hasTexCoords = texCoordChannel.Any();
				return new Mesh.Vertex(
					ToOpenTK(sceneMesh.Vertices[index]),
					ToOpenTK(sceneMesh.Normals[index]),
					hasTexCoords ? ToOpenTK(texCoordChannel[index]).Xy : Vector2.Zero
				);
			}
		}

		var couldCreate = packagedModelFile.Create(FileType.File);
		if (!couldCreate)
		{
			return false;
		}

		using var fileStream = packagedModelFile.Write();
		using var output = new BinaryWriter(fileStream);
		foreach (var (indices, vertices) in meshes)
		{
			output.Write((uint)indices.Length);
			output.Write((uint)vertices.Length);
			foreach (var index in indices)
			{
				output.Write((uint)index);
			}

			foreach (var ((pX, pY, pZ), (nX, nY, nZ), (u, v)) in vertices)
			{
				output.Write(pX);
				output.Write(pY);
				output.Write(pZ);
				output.Write(nX);
				output.Write(nY);
				output.Write(nZ);
				output.Write(u);
				output.Write(v);
			}
		}

		return true;
	}

	private bool TryLoadPackagedModelV2(FileHandle packagedModelFile, [NotNullWhen(true)] out object? model)
	{
		var meshes = new List<Mesh>();
		var currentIndices = new List<uint>();
		var currentVertices = new List<Mesh.Vertex>();
		using var fileStream = packagedModelFile.Read();
		using var reader = new BinaryReader(fileStream);

		var length = reader.BaseStream.Length;
		while (reader.BaseStream.Position < length)
		{
			currentIndices.Clear();
			currentVertices.Clear();

			var meshIndexCount = reader.ReadUInt32();
			var meshVertexCount = reader.ReadUInt32();
			for (var i = 0; i < meshIndexCount; i++)
			{
				currentIndices.Add(reader.ReadUInt32());
			}

			for (var i = 0; i < meshVertexCount; i++)
			{
				var position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
				var normal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
				var texCoord = new Vector2(reader.ReadSingle(), reader.ReadSingle());
				currentVertices.Add(new Mesh.Vertex(position, normal, texCoord));
			}

			var meshCreator = MainThread.Post(() => new Mesh(currentVertices, currentIndices, materialProvider()));
			meshCreator.WaitWhileHandlingMainThreadTasks();
			meshes.Add(meshCreator.Result);
		}

		model = new Model(meshes);
		return true;
	}

	private static Vector3 ToOpenTK(Vector3D assimpVector) => new(assimpVector.X, assimpVector.Y, assimpVector.Z);

	private record PackagedMeshIntermediateFormat(int[] Indices, Mesh.Vertex[] Vertices);
}