using FluentAssertions;
using NexusMods.DataModel.Extensions;
using NexusMods.DataModel.Loadouts.ApplySteps;
using NexusMods.DataModel.Loadouts.IngestSteps;
using NexusMods.DataModel.Loadouts.Markers;
using NexusMods.DataModel.Loadouts.ModFiles;
using NexusMods.DataModel.Tests.Harness;
using NexusMods.Hashing.xxHash64;
using NexusMods.Paths;
using BackupFile = NexusMods.DataModel.Loadouts.ApplySteps.BackupFile;
using RemoveFromLoadout = NexusMods.DataModel.Loadouts.ApplySteps.RemoveFromLoadout;

namespace NexusMods.DataModel.Tests;

public class ApplicationTests : ADataModelTest<ApplicationTests>
{

    public ApplicationTests(IServiceProvider provider) : base(provider)
    {

    }

    [Fact]
    public async Task CanApplyGame()
    {
        var mainList = await LoadoutManager.ManageGameAsync(Install, "MainList", CancellationToken.None);
        var hash = await ArchiveAnalyzer.AnalyzeFileAsync(DataZipLzma, CancellationToken.None);
        await ArchiveInstaller.AddMods(mainList.Value.LoadoutId, hash.Hash, "First Mod", CancellationToken.None);

        var plan = await ValidateSuccess(mainList.Value);
        plan.ToExtract.Count.Should().Be(3);

        await LoadoutSynchronizer.Apply(plan, CancellationToken.None);
        
        var gameFolder = Install.Locations[GameFolderType.Game];
        foreach (var file in DataNames)
        {
            gameFolder.Combine(file).FileExists.Should().BeTrue("File has been applied");
        }

        var newPlan = await ValidateSuccess(mainList.Value);
        newPlan.ToExtract.Count().Should().Be(0);
    }

    [Fact]
    public async Task CanIntegrateChanges()
    {
        var mainList = BaseList;
        await AddMods(mainList, DataZipLzma, "First Mod");
        await AddMods(mainList, Data7ZLzma2, "Second Mod");


        var originalPlan = await ValidateSuccess(mainList.Value);
        originalPlan.ToExtract.Count().Should().Be(3, "Files override each other");
        
        await LoadoutSynchronizer.Apply(originalPlan, Token);
        
        var gameFolder = Install.Locations[GameFolderType.Game];
        foreach (var file in DataNames)
        {
            gameFolder.Combine(file).FileExists.Should().BeTrue("File has been applied");
        }

        var fileToDelete = DataNames.First();
        var fileToModify = DataNames.Skip(1).First();

        gameFolder.Combine(fileToDelete).Delete();
        await gameFolder.Combine(fileToModify).WriteAllTextAsync("modified");
        var modifiedHash = "modified".XxHash64AsUtf8();

        var firstMod = mainList.Value.Mods.Values.First();
        var ingestPlan = await IngestSuccess(mainList.Value, _ => firstMod.Id);

        ingestPlan.IsForking.Should().BeFalse("there are no file conflicts");
        ingestPlan.ToUpdate.Should().ContainKey(gameFolder.Combine(fileToModify));
        ingestPlan.ToDelete.Should().Contain(Install.ToGamePath(gameFolder.Combine(fileToDelete)));



        /*
        (await LoadoutSynchronizer.FlattenLoadout(mainList.Value)).Files.Count.Should().Be(7, "because no changes are applied yet");

        await LoadoutSynchronizer.Ingest(ingestPlan);
        
        var flattened = (await LoadoutSynchronizer.FlattenLoadout(mainList.Value)).Files.Count;
        flattened.Should().Be(6, "Because we've deleted one file");

        mainList.Value.Mods.Values
            .SelectMany(m => m.Files.Values)
            .OfType<IToFile>()
            .OfType<IFromArchive>()
            .Where(f => f.Hash == modifiedHash)
            .Should()
            .NotBeEmpty("Because we've updated a file");
            
            */

    }

}
