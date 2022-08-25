namespace Natural_Graphics_Primitives_UI.Interfaces;

public interface IDataStorage
{
    public string DataPath { get; }
    public bool SaveData<T>(T data);
    public T LoadData<T>();
}