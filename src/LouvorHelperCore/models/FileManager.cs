using System.Text;
using System.Text.Json;

namespace LouvorHelperCore.Models;

public class FileManager
{
    public FileManager()
    {
        AppDataPath = Path.GetFullPath(
            Path.Join(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LouvorHelper"
            )
        );
        InitializeDirectory(AppDataPath);

        DownloadPath = Path.GetFullPath(Path.Join(AppDataPath, "Downloads"));
        InitializeDirectory(DownloadPath);

        CompileOutputPath = Path.GetFullPath(Path.Join(AppDataPath, "Compiled"));
        InitializeDirectory(CompileOutputPath);

        _jsonOptions = new JsonSerializerOptions();
        _jsonOptions.WriteIndented = true;
        _jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper;
    }

    public readonly string AppDataPath;
    public readonly string DownloadPath;
    public readonly string CompileOutputPath;

    private readonly JsonSerializerOptions _jsonOptions;

    public async Task SaveAsync(Music music)
    {
        string filePath = Path.Combine(DownloadPath, ApropriateFileName(music, ".json"));

        string json = JsonSerializer.Serialize(music, _jsonOptions);

        Directory.CreateDirectory(DownloadPath);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<Music?> LoadAsync(string filePath)
    {
        if (Path.Exists(filePath))
        {
            return JsonSerializer.Deserialize<Music>(
                await File.ReadAllTextAsync(filePath, Encoding.UTF8),
                _jsonOptions
            );
        }
        return null;
    }

    public IEnumerable<string> GetMusicFiles()
    {
        foreach (string fileName in Directory.EnumerateFiles(DownloadPath))
        {
            if (fileName is not null)
                yield return fileName;
        }
    }

    public async IAsyncEnumerable<Music> LoadAsync()
    {
        if (!Directory.Exists(DownloadPath))
            yield break;

        foreach (string fileName in Directory.EnumerateFiles(DownloadPath))
        {
            Music? music = await LoadAsync(fileName);

            if (music is not null)
                yield return music;
        }
    }

    public void InitializeDirectory(string directory)
    {
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }

    public void Clear(string directory)
    {
        InitializeDirectory(directory);
        Directory.Delete(directory, true);
        Directory.CreateDirectory(directory);
    }

    public void ClearDownloads() => Clear(DownloadPath);

    public void ClearCompiled() => Clear(CompileOutputPath);

    public string ApropriateFileName(Music music, string extension = ".pptx")
    {
        string titleFormatted = music.Title.ToUpper().Trim().Replace(' ', '_');
        string artistFormatted = music.Artist.ToLower().Trim().Replace(' ', '_');
        string fileName = $"{titleFormatted}-{artistFormatted}{extension}";
        return fileName;
    }
}
