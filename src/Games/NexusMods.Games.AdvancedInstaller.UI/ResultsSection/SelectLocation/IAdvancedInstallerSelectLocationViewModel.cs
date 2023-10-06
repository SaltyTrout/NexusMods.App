﻿using System.Collections.ObjectModel;
using NexusMods.App.UI;

namespace NexusMods.Games.AdvancedInstaller.UI.SelectLocation;

public interface IAdvancedInstallerSelectLocationViewModel : IViewModel
{
    public ReadOnlyObservableCollection<IAdvancedInstallerSuggestedEntryViewModel> SuggestedEntries { get; }
}