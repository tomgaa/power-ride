using Godot;
using System;

public partial class FTMSRowerSimulator : Node
{
	// FTMS Rower-specific data fields
	private float strokeRate = 0.0f;           // Strokes per minute
	private int strokeCount = 0;                // Total stroke count
	private float averageStrokeRate = 0.0f;    // Average strokes per minute
	private int totalDistance = 0;              // Total distance in meters
	private int instantaneousPace = 0;          // Current pace (seconds per 500m)
	private int averagePace = 0;                // Average pace (seconds per 500m)
	private int instantaneousPower = 0;         // Current power in watts
	private int averagePower = 0;               // Average power in watts
	private int totalEnergy = 0;                // Total energy in kcal
	private int energyPerHour = 0;              // Energy per hour in kcal
	private int energyPerMinute = 0;            // Energy per minute in kcal
	private int heartRate = 0;                  // Heart rate in BPM
	private int elapsedTime = 0;                // Elapsed time in seconds
	private int remainingTime = 0;              // Remaining time in seconds

	// Base values for simulation
	[Export] public float BaseStrokeRate { get; set; } = 24.0f;  // SPM (strokes per minute)
	[Export] public int BasePower { get; set; } = 150;            // Watts
	[Export] public int BaseHeartRate { get; set; } = 130;        // BPM
	[Export] public int BasePace { get; set; } = 120;             // seconds per 500m (2:00/500m)

	// Variation ranges for realistic simulation
	[Export] public float StrokeRateVariation { get; set; } = 2.0f;
	[Export] public int PowerVariation { get; set; } = 15;
	[Export] public int HeartRateVariation { get; set; } = 5;
	[Export] public int PaceVariation { get; set; } = 5;

	// Simulation state
	private float timeSinceLastStroke = 0.0f;
	private float strokeInterval = 0.0f;
	private float totalTimeElapsed = 0.0f;
	private float distancePerStroke = 10.0f;  // meters per stroke (adjustable)
	private bool isRunning = false;  // Track if the simulator is running

	// FTMS Rower Data signal - matches FTMS specification
	[Signal]
	public delegate void RowerDataUpdatedEventHandler(
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
		int remainingTime
	);
	public override void _Ready()
	{
		SetProcess(true);
		strokeInterval = 60.0f / BaseStrokeRate; // Calculate time between strokes
	}

	public override void _Process(double delta)
	{
		if (!isRunning)
		{
			return; // Don't process if not running
		}
		
		totalTimeElapsed += (float)delta;
		timeSinceLastStroke += (float)delta;
		elapsedTime = (int)totalTimeElapsed;

		// Simulate strokes based on stroke rate
		strokeInterval = 60.0f / (BaseStrokeRate + (float)GD.RandRange(-StrokeRateVariation, StrokeRateVariation));
		
		if (timeSinceLastStroke >= strokeInterval)
		{
			// New stroke
			strokeCount++;
			timeSinceLastStroke = 0.0f;
			
			// Update distance
			totalDistance += (int)distancePerStroke;
		}

		// Calculate current stroke rate (strokes per minute)
		if (strokeInterval > 0)
		{
			strokeRate = 60.0f / strokeInterval;
			strokeRate = Mathf.Max(0.0f, strokeRate);
		}

		// Calculate average stroke rate
		if (totalTimeElapsed > 0)
		{
			averageStrokeRate = (strokeCount / totalTimeElapsed) * 60.0f;
		}

		// Simulate instantaneous power with variation
		instantaneousPower = BasePower + GD.RandRange(-PowerVariation, PowerVariation);
		instantaneousPower = Mathf.Max(0, instantaneousPower);

		// Calculate average power
		if (elapsedTime > 0)
		{
			averagePower = (averagePower * (elapsedTime - 1) + instantaneousPower) / elapsedTime;
		}

		// Calculate instantaneous pace (seconds per 500m)
		// Pace is inversely related to power and stroke rate
		instantaneousPace = BasePace + GD.RandRange(-PaceVariation, PaceVariation);
		instantaneousPace = Mathf.Max(60, instantaneousPace); // Minimum 1:00/500m

		// Calculate average pace
		if (totalDistance > 0 && totalTimeElapsed > 0)
		{
			averagePace = (int)((totalTimeElapsed / (totalDistance / 500.0f)));
		}

		// Simulate heart rate
		heartRate = BaseHeartRate + GD.RandRange(-HeartRateVariation, HeartRateVariation);
		heartRate = Mathf.Max(60, heartRate);

		// Calculate energy (simplified: ~1 kcal per 4 watts per minute)
		totalEnergy = (int)((averagePower * totalTimeElapsed) / 240.0f);
		energyPerHour = (int)((averagePower * 60.0f) / 4.0f);
		energyPerMinute = energyPerHour / 60;

		// Emit the updated rower data
		EmitSignal(SignalName.RowerDataUpdated,
			strokeRate,
			strokeCount,
			averageStrokeRate,
			totalDistance,
			instantaneousPace,
			averagePace,
			instantaneousPower,
			averagePower,
			totalEnergy,
			energyPerHour,
			energyPerMinute,
			heartRate,
			elapsedTime,
			remainingTime
		);
	}

	// Control methods for simulating resistance changes
	public void SetResistance(float level)
	{
		// Level from 0.0 (easy) to 1.0 (hard)
		// Higher resistance = more power needed, slower pace
		BasePower = (int)(80 + (level * 200));    // 80-280 watts
		BasePace = (int)(90 + (level * 60));      // 1:30 to 2:30 per 500m
		distancePerStroke = 8.0f + (level * 4.0f); // 8-12 meters per stroke
	}

	public void IncreaseEffort()
	{
		BaseStrokeRate = Mathf.Min(40.0f, BaseStrokeRate + 2.0f);
		BasePower += 20;
		BaseHeartRate += 5;
		BasePace = Mathf.Max(60, BasePace - 5);
	}

	public void DecreaseEffort()
	{
		BaseStrokeRate = Mathf.Max(10.0f, BaseStrokeRate - 2.0f);
		BasePower = Mathf.Max(50, BasePower - 20);
		BaseHeartRate = Mathf.Max(80, BaseHeartRate - 5);
		BasePace = Mathf.Min(180, BasePace + 5);  // Max 3:00/500m (slower pace)
	}

	public void Reset()
	{
		isRunning = false;
		strokeCount = 0;
		totalDistance = 0;
		totalEnergy = 0;
		totalTimeElapsed = 0.0f;
		elapsedTime = 0;
		averageStrokeRate = 0.0f;
		averagePower = 0;
		averagePace = 0;
		timeSinceLastStroke = 0.0f;
		strokeRate = 0.0f;
		instantaneousPace = 0;
		instantaneousPower = 0;
		heartRate = 0;
		energyPerHour = 0;
		energyPerMinute = 0;
		remainingTime = 0;
		
		// Emit signal with all zeros to update UI
		EmitSignal(SignalName.RowerDataUpdated,
			0.0f,  // strokeRate
			0,     // strokeCount
			0.0f,  // averageStrokeRate
			0,     // totalDistance
			0,     // instantaneousPace
			0,     // averagePace
			0,     // instantaneousPower
			0,     // averagePower
			0,     // totalEnergy
			0,     // energyPerHour
			0,     // energyPerMinute
			0,     // heartRate
			0,     // elapsedTime
			0      // remainingTime
		);
	}

	public void Start()
	{
		isRunning = true;
	}

	public void Stop()
	{
		isRunning = false;
	}

	public bool IsRunning()
	{
		return isRunning;
	}

	// Getters for current values
	public float GetStrokeRate() => strokeRate;
	public int GetStrokeCount() => strokeCount;
	public int GetTotalDistance() => totalDistance;
	public int GetPower() => instantaneousPower;
	public int GetHeartRate() => heartRate;
	public int GetPace() => instantaneousPace;
}
