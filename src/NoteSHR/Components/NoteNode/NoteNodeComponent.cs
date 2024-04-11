﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using NoteSHR.Components.NoteNode.EventArgs;
using NoteSHR.ViewModels;

namespace NoteSHR.Components.NoteNode;

public class NoteNodeComponent : UserControl
{
    private const string MoveUpButtonName = "MoveUpButton";
    private const string MoveDownButtonName = "MoveDownButton";

    public static readonly StyledProperty<ObservableCollection<NodeViewModel>> NodesProperty =
        AvaloniaProperty.Register<NoteNodeComponent, ObservableCollection<NodeViewModel>>(nameof(Nodes),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<Guid> NoteIdProperty =
        AvaloniaProperty.Register<NoteNodeComponent, Guid>(nameof(NoteId), defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> DeleteModeProperty =
        AvaloniaProperty.Register<NoteNodeComponent, bool>(nameof(DeleteMode), defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> EditModeProperty =
        AvaloniaProperty.Register<NoteNodeComponent, bool>(nameof(EditMode), defaultBindingMode: BindingMode.OneWay);

    public static readonly RoutedEvent<DeleteNodeEventArgs> DeleteNodeEvent =
        RoutedEvent.Register<NoteNodeComponent, DeleteNodeEventArgs>(nameof(DeleteNode), RoutingStrategies.Direct);

    public static readonly RoutedEvent<MoveNodeEventArgs> MoveNodeEvent =
        RoutedEvent.Register<NoteNodeComponent, MoveNodeEventArgs>(nameof(MoveNodeEvent), RoutingStrategies.Direct);

    private readonly StackPanel _stackPanel;

    public NoteNodeComponent()
    {
        MinHeight = 200;
        _stackPanel = new StackPanel
        {
            Name = "Node",
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        Content = _stackPanel;
    }

    private ObservableCollection<NodeViewModel> Nodes
    {
        get => GetValue(NodesProperty);
        set => SetValue(NodesProperty, value);
    }

    private Guid NoteId
    {
        get => GetValue(NoteIdProperty);
        set => SetValue(NoteIdProperty, value);
    }

    private bool DeleteMode
    {
        get => GetValue(DeleteModeProperty);
        set => SetValue(DeleteModeProperty, value);
    }

    private bool EditMode
    {
        get => GetValue(EditModeProperty);
        set => SetValue(EditModeProperty, value);
    }

    public event EventHandler<DeleteNodeEventArgs> DeleteNode
    {
        add => AddHandler(DeleteNodeEvent, value);
        remove => RemoveHandler(DeleteNodeEvent, value);
    }

    public event EventHandler<MoveNodeEventArgs> MoveNode
    {
        add => AddHandler(MoveNodeEvent, value);
        remove => RemoveHandler(MoveNodeEvent, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == NodesProperty) Console.Write("AAA  ");
    }

    protected override void OnInitialized()
    {
        foreach (var nodeVm in Nodes)
        {
            var grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                DataContext = nodeVm.Id,
                ColumnDefinitions = new ColumnDefinitions
                {
                    new(20.0, GridUnitType.Auto),
                    new(GridLength.Star)
                },
                RowDefinitions = new RowDefinitions
                {
                    new(GridLength.Auto)
                }
            };

            if (DeleteMode)
            {
                var deleteButton = new Button
                {
                    Content = "D",
                    DataContext = nodeVm
                };

                Grid.SetColumn(deleteButton, 0);

                deleteButton.Click += (sender, args) =>
                    RaiseEvent(new DeleteNodeEventArgs(DeleteNodeEvent, NoteId, nodeVm.Id));
                grid.Children.Add(deleteButton);
            }

            if (EditMode)
            {
                var editModeGrid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions
                    {
                        new(GridLength.Star)
                    },
                    RowDefinitions = new RowDefinitions
                    {
                        new(GridLength.Auto),
                        new(GridLength.Auto)
                    }
                };

                var moveUpButton = new TextBlock
                {
                    Name = MoveUpButtonName,
                    Text = "\u25b2",
                    DataContext = nodeVm.Id
                };

                var moveDownButton = new TextBlock
                {
                    Name = MoveDownButtonName,
                    Text = "\u25bc",
                    DataContext = nodeVm.Id
                };

                moveUpButton.PointerPressed += EditModeButtonClicked;
                moveDownButton.PointerPressed += EditModeButtonClicked;

                Grid.SetRow(moveUpButton, 0);
                Grid.SetColumn(moveUpButton, 0);
                Grid.SetRow(moveDownButton, 1);
                Grid.SetColumn(moveDownButton, 0);

                Grid.SetColumn(editModeGrid, 0);

                editModeGrid.Children.Add(moveUpButton);
                editModeGrid.Children.Add(moveDownButton);

                grid.Children.Add(editModeGrid);
            }

            var node = (Control)Activator.CreateInstance(nodeVm.Type, nodeVm.ViewModel);
            node.HorizontalAlignment = HorizontalAlignment.Stretch;
            node.VerticalAlignment = VerticalAlignment.Stretch;
            Grid.SetColumn(node, 1);

            grid.Children.Add(node);
            _stackPanel.Children.Add(grid);
        }

        Content = _stackPanel;
    }

    private void EditModeButtonClicked(object? sender, PointerPressedEventArgs e)
    {
        var control = e.Source as TextBlock;
        var sourceNode = Nodes.SingleOrDefault(x => x.Id == (Guid)control.DataContext);
        var moveOptions = control?.Name == MoveUpButtonName ? NodeMoveOptions.Up : NodeMoveOptions.Down;

        RaiseEvent(new MoveNodeEventArgs(MoveNodeEvent, NoteId, sourceNode.Id, moveOptions));
    }
}