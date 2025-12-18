using Godot;

public partial class MainMenu : Control
{
    [Export] public PackedScene GameScene;
    [Export] public PackedScene ScoreboardScene;
    [Export] public PackedScene OptionsScene;

    private AcceptDialog _infoDialog;

    public override void _Ready()
    {
        // Hook up buttons
        GetButton("StartButton").Pressed += OnStartPressed;
        GetButton("ScoreboardButton").Pressed += OnScoreboardPressed;
        GetButton("OptionsButton").Pressed += OnOptionsPressed;
        GetButton("ExitButton").Pressed += OnExitPressed;

        // Lightweight info dialog for placeholders
        _infoDialog = new AcceptDialog
        {
            Title = "Info"
        };
        AddChild(_infoDialog);

        // Provide sensible defaults so it works out of the box
        GameScene ??= GD.Load<PackedScene>("res://scenes/ui/RowerSimulatorUI.tscn");
    }

    private Button GetButton(string name)
    {
        return GetNode<Button>("CenterContainer/Panel/Margin/VBox/Buttons/" + name);
    }

    private void OnStartPressed()
    {
        if (GameScene != null)
        {
            GetTree().ChangeSceneToPacked(GameScene);
        }
        else
        {
            ShowInfo("Brak przypisanej sceny gry.");
        }
    }

    private void OnScoreboardPressed()
    {
        if (ScoreboardScene != null)
        {
            GetTree().ChangeSceneToPacked(ScoreboardScene);
        }
        else
        {
            ShowInfo("Scoreboard coming soon."); 
        }
    }

    private void OnOptionsPressed()
    {
        if (OptionsScene != null)
        {
            GetTree().ChangeSceneToPacked(OptionsScene);
        }
        else
        {
            ShowInfo("Options coming soon.");
        }
    }

    private void OnExitPressed()
    {
        GetTree().Quit();
    }

    private void ShowInfo(string text)
    {
        _infoDialog.DialogText = text;
        _infoDialog.PopupCentered();
    }
}
