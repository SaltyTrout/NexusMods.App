﻿namespace NexusMods.App.UI.Controls.WorkspaceGrid;

public class PaneDefinition
{
    public double Left { get; set; }
    public double Top { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    
    public double ActualLeft { get; set; }
    public double ActualTop { get; set; }
    public double ActualWidth { get; set; }
    public double ActualHeight { get; set; }

    /// <summary>
    /// Makes a copy of this pane
    /// </summary>
    /// <returns></returns>
    public PaneDefinition Clone()
    {
        return new PaneDefinition()
        {
            Left = Left,
            Top = Top,
            Width = Width,
            Height = Height
        };
    }
    
    public double Area => Width * Height;
}
