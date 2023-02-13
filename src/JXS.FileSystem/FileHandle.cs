using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using JXS.Utils;

namespace JXS.FileSystem;

public record FileHandle(string FilePath)
{
	private static readonly string Separator = Path.DirectorySeparatorChar == '\\'
		? $"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}"
		: $"{Path.DirectorySeparatorChar}";

	private static readonly Regex OneDotRegex =
		new($"{Separator}\\.{Separator}", RegexOptions.Compiled);

	private static readonly Regex TwoDotsRegex =
		new($"[^{Separator}]+{Separator}\\.\\.{Separator}?", RegexOptions.Compiled);

	public string FilePath { get; } = Minimize(Sanitize(FilePath));

	public virtual FileType Type => File.Exists(FilePath)
		? FileType.File
		: Directory.Exists(FilePath)
			? FileType.Directory
			: FileType.Invalid;

	public virtual bool Exists => Type != FileType.Invalid;

	public string Extension { get; } = Path.GetExtension(Minimize(Sanitize(FilePath)));

	public virtual string ReadAllText() => Type == FileType.File
		? File.ReadAllText(FilePath)
		: DevTools.DebugReturn<string, FileHandle>(string.Empty,
			new FileNotFoundException($"Tried to {nameof(ReadAllText)} on {this}."));

	public virtual byte[] ReadAllBytes() => Type == FileType.File
		? File.ReadAllBytes(FilePath)
		: DevTools.DebugReturn<byte[], FileHandle>(Array.Empty<byte>(),
			new FileNotFoundException($"Tried to {nameof(ReadAllBytes)} on {this}."));

	public virtual FileStream Read() => Type == FileType.File
		? File.OpenRead(FilePath)
		: DevTools.DebugReturn<FileStream, FileHandle>((FileStream)Stream.Null,
			new FileNotFoundException($"Tried to {nameof(Read)} on {this}."));

	public virtual StreamReader ReadText() => Type == FileType.File
		? File.OpenText(FilePath)
		: DevTools.DebugReturn<StreamReader, FileHandle>(StreamReader.Null,
			new FileNotFoundException($"Tried to {nameof(ReadText)} on {this}."));

	public virtual FileStream Write() => Type == FileType.File
		? File.OpenWrite(FilePath)
		: DevTools.DebugReturn<FileStream, FileHandle>((FileStream)Stream.Null,
			new FileNotFoundException($"Tried to {nameof(Write)} on {this}."));

	/// <summary>
	///     Attempts to write the given text string to the file at this location. If the text could not be written
	///     (e.g. if the file could not be opened, or if this <see cref="FileHandle" /> points to a directory)
	///     this function will return <c>false</c>, otherwise if it succeeded it returns <c>true</c>.
	/// </summary>
	/// <remarks>
	///     This method will not try to create the file for you; call <see cref="Create" /> first. This method will
	///     override the contents of the file.
	/// </remarks>
	/// <param name="content"></param>
	/// <param name="encoding"></param>
	/// <returns></returns>
	public bool WriteAllText(string content, Encoding? encoding = null)
	{
		if (Type != FileType.File)
		{
			return false;
		}

		try
		{
			if (encoding == null)
			{
				File.WriteAllText(FilePath, content);
			}
			else
			{
				File.WriteAllText(FilePath, content, encoding);
			}

			return true;
		}
		catch
		{
			return false;
		}
	}

	public FileHandle Parent() => new(Path.Combine(FilePath, path2: ".."));
	public FileHandle Child(string path) => new(Path.Combine(FilePath, Sanitize(path)));
	public FileHandle Sibling(string path) => Parent().Child(path);
	public FileHandle AppendToPath(string extra) => new($"{FilePath}{extra}");

	public FileHandle SiblingWithDifferentExtension(string newExtension) =>
		new(Path.ChangeExtension(FilePath, newExtension));

	public bool HasExtension(params string[] extensions) => extensions.Length == 0
		? Path.HasExtension(FilePath)
		: extensions.Contains(Path.GetExtension(FilePath));

	public IEnumerable<FileHandle> GetAllChildren() => Type == FileType.Directory
		? Directory.GetFileSystemEntries(FilePath).Select(path => new FileHandle(path))
		: Enumerable.Empty<FileHandle>();

	public IEnumerable<FileHandle> GetChildrenOfType(FileType type) => Type == FileType.Directory
		? type switch
		{
			FileType.Directory => Directory.GetDirectories(FilePath).Select(path => new FileHandle(path)),
			FileType.File => Directory.GetFiles(FilePath).Select(path => new FileHandle(path)),
			FileType.Invalid => Enumerable.Empty<FileHandle>(),
			_ => DevTools.DebugReturn<IEnumerable<FileHandle>, FileHandle>(Enumerable.Empty<FileHandle>(),
				new ArgumentOutOfRangeException(nameof(type), type, message: null))
		}
		: Enumerable.Empty<FileHandle>();

	public bool Create(FileType typeHint = FileType.Invalid)
	{
		if (Type != FileType.Invalid)
		{
			return true;
		}

		var assumedFileType = Path.HasExtension(FilePath) ? FileType.File : FileType.Directory;
		var creationType = typeHint == FileType.Invalid
			? assumedFileType
			: typeHint;

		try
		{
			return creationType switch
			{
				FileType.Directory => Directory.CreateDirectory(FilePath).Exists,
				FileType.File => CreateFile(FilePath),
				_ => DevTools.DebugReturn<bool, FileHandle>(value: false,
					new InvalidEnumArgumentException(nameof(creationType), (int)creationType, typeof(FileType)))
			};
		}
		catch (Exception e)
		{
			DevTools.ThrowStatic(e);
			return false;
		}
	}

	private static bool CreateFile(string path)
	{
		using var file = File.Create(path);
		return file.CanRead;
	}

	public void Clear()
	{
		// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
		switch (Type)
		{
			case FileType.Directory:
				foreach (var child in GetAllChildren())
				{
					child.Delete();
				}

				break;
			case FileType.File:
				File.WriteAllBytes(FilePath, Array.Empty<byte>());
				break;
		}
	}

	public void Delete(bool recursive = true)
	{
		// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
		switch (Type)
		{
			case FileType.Directory:
				Directory.Delete(FilePath, recursive);
				break;
			case FileType.File:
				File.Delete(FilePath);
				break;
		}
	}

	public override string ToString() =>
		$"{nameof(FileHandle)}{{{nameof(FilePath)}: {FilePath}, {nameof(Type)}: {Type}, {nameof(Exists)}: {Exists}}}";

	private static string Sanitize(string path) =>
		Path.Combine(path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

	private static string Minimize(string path)
	{
		while (OneDotRegex.IsMatch(path) || TwoDotsRegex.IsMatch(path))
		{
			path = OneDotRegex.Replace(path, replacement: "/");
			path = TwoDotsRegex.Replace(path, replacement: "");
		}

		return path;
	}
}