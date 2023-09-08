namespace CodeFirst.Gui;

public interface IResourceProvider
{
	T Load<T>(string path);
}