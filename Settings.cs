using Godot;

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperShedAdmin;

public class Settings {

	public static Settings Instance { get; set; } =
		Load() ?? new();

	public static string Path => ProjectSettings.GlobalizePath("user://settings.json");

	public static JsonSerializerOptions JsonSerializerOptions { get; set; } = new() {

		WriteIndented = true,
		IncludeFields = true

	};

	[JsonInclude]
	public string? authToken;
	[JsonIgnore]
	public virtual string? AuthToken {

		get => authToken;

		set {

			authToken = value;

			Save();

		}

	}

	public static Settings? Load() {

		Settings? settings = null;

		if(!File.Exists(Path)) {

			return null;

		}

		try {

			settings =
				JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path),
														JsonSerializerOptions);

		}

		catch(Exception e) {

			GD.PushError($"Something went wrong when loading the settings: {e}");

		}

		return settings;

	}

	public static void Save() {

		try {

			string? settingsDirectory =
				System.IO.Path.GetDirectoryName(Path) ??
					throw new("Failed to create settings directory!");

			Directory.CreateDirectory(settingsDirectory);

			File.WriteAllText(Path,
								JsonSerializer.Serialize(Instance,
																	JsonSerializerOptions));

		}

		catch(Exception e) {

			GD.PushError($"Something went wrong when saving the settings: {e}");

		}

	}

}