using Godot;

using System;
using System.IO;
using System.Text.Json;

namespace SuperShedAdmin;

public class Settings {

	public static Settings Instance { get; set; } = new(true);

	public static string Path => ProjectSettings.GlobalizePath("user://settings.json");

	public static JsonSerializerOptions JsonSerializerOptions { get; set; } = new() {

		WriteIndented = true

	};

	private string? authToken;
	public virtual string? AuthToken {

		get => authToken;

		set {

			authToken = value;

			Save();

		}

	}

	public Settings(bool load = false) {

		if(!load) {

			return;

		}

		Load();

	}

	public static Settings Load() {

		Settings settings = new();

		try {

			if(!File.Exists(Path)) {

				return settings;

			}

			return Instance = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path),
																	JsonSerializerOptions)!;

		}

		catch(Exception e) {

			GD.PushError($"Something went wrong when loading the settings: {e}");

		}

		return settings;

	}

	public static void Save() {

		try {

			Directory.CreateDirectory(Path);

			File.WriteAllText(Path,
								JsonSerializer.Serialize(Instance,
																	JsonSerializerOptions));

		}

		catch(Exception e) {

			GD.PushError($"Something went wrong when saving the settings: {e}");

		}

	}

}