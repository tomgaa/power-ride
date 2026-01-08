using Godot;

public partial class Game : Node3D
{
	private FTMSRowerSimulator simulator;
	private Label dataDisplayLabel;
	private Button startStopButton;
	private Button increaseButton;
	private Button decreaseButton;
	private Button resetButton;
	
	public override void _Ready()
	{
		// Create and add simulator
		simulator = new FTMSRowerSimulator();
		AddChild(simulator);
		
		// Connect to data update signal
		simulator.RowerDataUpdated += OnRowerDataUpdated;
		
		// Get UI references
		dataDisplayLabel = GetNode<Label>("UI/DataDisplayLabel");
		startStopButton = GetNode<Button>("UI/ButtonsContainer/StartStopButton");
		increaseButton = GetNode<Button>("UI/ButtonsContainer/IncreaseButton");
		decreaseButton = GetNode<Button>("UI/ButtonsContainer/DecreaseButton");
		resetButton = GetNode<Button>("UI/ButtonsContainer/ResetButton");
		
		// Connect button signals
		startStopButton.Pressed += OnStartStopButtonPressed;
		increaseButton.Pressed += OnIncreaseButtonPressed;
		decreaseButton.Pressed += OnDecreaseButtonPressed;
		resetButton.Pressed += OnResetButtonPressed;
		
		UpdateStartStopButton();
	}
	
	private void OnRowerDataUpdated(
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
		// Format pace
		int paceMinutes = instantaneousPace / 60;
		int paceSeconds = instantaneousPace % 60;
		string paceStr = $"{paceMinutes}:{paceSeconds:D2}/500m";
		
		int avgPaceMinutes = averagePace / 60;
		int avgPaceSeconds = averagePace % 60;
		string avgPaceStr = $"{avgPaceMinutes}:{avgPaceSeconds:D2}/500m";
		
		// Format time
		int minutes = elapsedTime / 60;
		int seconds = elapsedTime % 60;
		string timeStr = $"{minutes}:{seconds:D2}";
		
		// Format distance
		string distanceStr = $"{totalDistance}m ({totalDistance / 1000.0f:F2}km)";
		
		// Calculate speed in km/h
		float speedKmh = 0.0f;
		if (elapsedTime > 0)
		{
			speedKmh = (totalDistance / (float)elapsedTime) * 3.6f; // m/s to km/h
		}
		
		// Update display
		string displayText = $@"Stroke Rate: {strokeRate:F1} SPM
        Power: {instantaneousPower}W
        Pace: {paceStr}
        Speed: {speedKmh:F1} km/h
        Heart Rate: {heartRate} BPM
        Time: {timeStr}
        Distance: {distanceStr}
        Strokes: {strokeCount}
        Calories: {totalEnergy} kcal
        Avg SR: {averageStrokeRate:F1} SPM
        Avg Power: {averagePower}W
        Avg Pace: {avgPaceStr}";
		
		dataDisplayLabel.Text = displayText;
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
