using Godot;

public partial class RowerSimulatorUI : Control
{
	private FTMSRowerSimulator simulator;
	
	private Label strokeRateLabel;
	private Label strokeCountLabel;
	private Label distanceLabel;
	private Label paceLabel;
	private Label powerLabel;
	private Label heartRateLabel;
	private Label timeLabel;
	private Label caloriesLabel;
	private Label avgStrokeRateLabel;
	private Label avgPaceLabel;
	private Label avgPowerLabel;
	private Button startStopButton;

	public override void _Ready()
	{
		// Create and add simulator
		simulator = new FTMSRowerSimulator();
		AddChild(simulator);
		
		// Connect to data update signal
		simulator.RowerDataUpdated += OnRowerDataUpdated;
		
		// Get UI references
		strokeRateLabel = GetNode<Label>("VBoxContainer/StrokeRateLabel");
		strokeCountLabel = GetNode<Label>("VBoxContainer/StrokeCountLabel");
		distanceLabel = GetNode<Label>("VBoxContainer/DistanceLabel");
		paceLabel = GetNode<Label>("VBoxContainer/PaceLabel");
		powerLabel = GetNode<Label>("VBoxContainer/PowerLabel");
		heartRateLabel = GetNode<Label>("VBoxContainer/HeartRateLabel");
		timeLabel = GetNode<Label>("VBoxContainer/TimeLabel");
		caloriesLabel = GetNode<Label>("VBoxContainer/CaloriesLabel");
		avgStrokeRateLabel = GetNode<Label>("VBoxContainer/AvgStrokeRateLabel");
		avgPaceLabel = GetNode<Label>("VBoxContainer/AvgPaceLabel");
		avgPowerLabel = GetNode<Label>("VBoxContainer/AvgPowerLabel");
		startStopButton = GetNode<Button>("VBoxContainer/ButtonsContainer/StartStopButton");
		
		UpdateStartStopButton();
	}	private void OnRowerDataUpdated(
		float strokeRate,
		int strokeCount,
		float averageStrokeRate,
		int totalDistance,
		int instantaneousPace,
		int averagePace,
		int instantaneousPower,
		int averagePower,
		int totalEnergy,
		int energyPerHour,
		int energyPerMinute,
		int heartRate,
		int elapsedTime,
		int remainingTime)
	{
		// Update current values
		strokeRateLabel.Text = $"Stroke Rate: {strokeRate:F1} SPM";
		strokeCountLabel.Text = $"Strokes: {strokeCount}";
		distanceLabel.Text = $"Distance: {totalDistance}m ({totalDistance / 1000.0f:F2}km)";
		
		// Format pace as MM:SS per 500m
		int paceMinutes = instantaneousPace / 60;
		int paceSeconds = instantaneousPace % 60;
		paceLabel.Text = $"Pace: {paceMinutes}:{paceSeconds:D2}/500m";
		
		powerLabel.Text = $"Power: {instantaneousPower}W";
		heartRateLabel.Text = $"Heart Rate: {heartRate} BPM";
		
		// Format elapsed time
		int minutes = elapsedTime / 60;
		int seconds = elapsedTime % 60;
		timeLabel.Text = $"Time: {minutes}:{seconds:D2}";
		
		caloriesLabel.Text = $"Calories: {totalEnergy} kcal ({energyPerMinute} kcal/min)";
		
		// Update averages
		avgStrokeRateLabel.Text = $"Avg Stroke Rate: {averageStrokeRate:F1} SPM";
		
		int avgPaceMinutes = averagePace / 60;
		int avgPaceSeconds = averagePace % 60;
		avgPaceLabel.Text = $"Avg Pace: {avgPaceMinutes}:{avgPaceSeconds:D2}/500m";
		
		avgPowerLabel.Text = $"Avg Power: {averagePower}W";
	}

	private void OnIncreaseButtonPressed()
	{
		simulator.IncreaseEffort();
	}

	private void OnDecreaseButtonPressed()
	{
		simulator.DecreaseEffort();
	}

	private void OnResetButtonPressed()
	{
		simulator.Reset();
		UpdateStartStopButton();
	}

	private void OnStartStopButtonPressed()
	{
		if (simulator.IsRunning())
		{
			simulator.Stop();
		}
		else
		{
			simulator.Start();
		}
		UpdateStartStopButton();
	}

	private void UpdateStartStopButton()
	{
		if (simulator.IsRunning())
		{
			startStopButton.Text = "Stop";
		}
		else
		{
			startStopButton.Text = "Start";
		}
	}
}
