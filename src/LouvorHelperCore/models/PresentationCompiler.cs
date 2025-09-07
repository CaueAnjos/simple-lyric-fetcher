using LouvorHelperCore.Models.Presentation;
using LouvorHelperCore.Utils;

namespace LouvorHelperCore.Models;

public class PresentationCompiler
{
    public Queue<Music> Musics { get; private set; }
    public FileManager FileManager { get; private set; }
    public string TemplatePath { get; set; } = string.Empty;

    public PresentationCompiler(IEnumerable<Music> musics, FileManager? fileManager = null)
    {
        Musics = new Queue<Music>(musics);
        FileManager = fileManager is null ? new FileManager() : fileManager;
    }

    public PresentationCompiler(FileManager? fileManager = null)
    {
        Musics = new Queue<Music>();
        FileManager = fileManager is null ? new FileManager() : fileManager;
    }

    public void AddMusicToCompiler(Music music)
    {
        Musics.Enqueue(music);
    }

    public void AddMusicToCompiler(IEnumerable<Music> musics)
    {
        foreach (Music music in musics)
            Musics.Enqueue(music);
    }

    public void CompileMusic(Music music)
    {
        string filePath = Path.Combine(
            FileManager.CompileOutputPath,
            FileManager.ApropriateFileName(music, extension: ".pptx")
        );

        PresentationDocument presentation = new(music);
        presentation.SetTemplate(TemplatePath);
        presentation.Save(filePath);
    }

    public async Task CompileQueueAsync()
    {
        FileManager.ClearCompiled();

        int maxTasksPerCompileCicle = 5;
        List<Task> tasks = new List<Task>(maxTasksPerCompileCicle);

        foreach (Music music in Musics)
        {
            tasks.Add(
                Task.Run(() =>
                {
                    CompileMusic(music);
                })
            );

            if (tasks.Count >= maxTasksPerCompileCicle)
            {
                await Task.WhenAll(tasks);
                tasks.Clear();
            }
        }

        if (tasks.Count > 0)
            await Task.WhenAll(tasks);
    }

    public async Task CompileAllAsync()
    {
        await foreach (Music music in FileManager.LoadAsync())
        {
            Musics.Enqueue(music);
        }

        await CompileQueueAsync();
    }
}
