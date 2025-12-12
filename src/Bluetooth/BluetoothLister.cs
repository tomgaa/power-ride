using Godot;
using System;
using System.Threading.Tasks;
using InTheHand.Bluetooth;

public partial class BluetoothLister : Node
{
	public override void _Ready()
	{
		GD.Print("Scanning for BLE devices...");
		ListDevices();
	}

	private async void ListDevices()
	{
		try
		{
			// Get a collection of all nearby devices
			var devices = await Bluetooth.ScanForDevicesAsync();

			if (devices.Count == 0)
			{
				GD.Print("No BLE devices found.");
				return;
			}

			foreach (var device in devices)
			{
				string name = string.IsNullOrEmpty(device.Name) ? "(no name)" : device.Name;
				GD.Print($"Device: {name} | ID: {device.Id}");
			}
		}
		catch (Exception e)
		{
			GD.PrintErr("BLE Error: " + e.Message);
		}
	}
}
