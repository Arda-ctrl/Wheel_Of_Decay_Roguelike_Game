using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class GoblinAnimatorBuilder
{
	private const string BaseControllerName = "GoblinBase.controller";
	private const string OutputFolder = "Assets/Animations/Goblins/_Generated";
	private const string BaseClipsFolder = OutputFolder + "/BaseClips";

	private static readonly string[] SearchFolders =
	{
		"Assets/Animations",
		"Assets"
	};

	[MenuItem("Tools/Goblins/Build Animator Controllers")] 
	public static void BuildGoblinAnimators()
	{
		EnsureFolders();

		// 1) Create or load base AnimatorController
		var baseControllerPath = Path.Combine(OutputFolder, BaseControllerName).Replace("\\", "/");
		var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(baseControllerPath);
		if (controller == null)
		{
			controller = AnimatorController.CreateAnimatorControllerAtPath(baseControllerPath);
			SetupBaseController(controller);
		}

		// 2) Collect all clips once
		var allClips = LoadAllClips();

		// 3) Create overrides per type
		CreateOverride("DaggerGoblin_AOC", controller, MapDaggerGoblin(allClips));
		CreateOverride("DaggerGoblinBomber_AOC", controller, MapDaggerGoblinBomber(allClips));
		CreateOverride("TrapperGoblin_AOC", controller, MapTrapperGoblin(allClips));
		CreateOverride("TrapperGoblinBombs_AOC", controller, MapTrapperGoblinBombs(allClips));

		// 4) Create full controllers (no overrides) as requested
		CreateOrUpdateConcreteController("DaggerGoblin.controller", MapDaggerGoblin(allClips));
		CreateOrUpdateConcreteController("DaggerGoblinBomber.controller", MapDaggerGoblinBomber(allClips));
		CreateOrUpdateConcreteController("TrapperGoblin.controller", MapTrapperGoblin(allClips));
		CreateOrUpdateConcreteController("TrapperGoblinBombs.controller", MapTrapperGoblinBombs(allClips));

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		EditorUtility.DisplayDialog("Goblin Animators", "AnimatorController and overrides generated successfully.", "OK");
	}

	// --- Base Controller ---
	private static void SetupBaseController(AnimatorController controller)
	{
		// Parameters (boolean-driven locomotion)
		AddParam(controller, AnimatorControllerParameterType.Bool, "IsIdle");
		AddParam(controller, AnimatorControllerParameterType.Bool, "IsJogging");
		AddParam(controller, AnimatorControllerParameterType.Trigger, "Attack");
		AddParam(controller, AnimatorControllerParameterType.Bool, "IsAttacking");
		AddParam(controller, AnimatorControllerParameterType.Trigger, "Throw");
		AddParam(controller, AnimatorControllerParameterType.Bool, "IsSettingUp");
		AddParam(controller, AnimatorControllerParameterType.Bool, "IsDead");

		// Base placeholder clips to be overridden
		var baseIdle = CreateOrLoadBaseClip("Goblin_Base_Idle");
		var baseWalk = CreateOrLoadBaseClip("Goblin_Base_Walk");
		var baseJog = CreateOrLoadBaseClip("Goblin_Base_Jog");
		var baseAttack = CreateOrLoadBaseClip("Goblin_Base_Attack");
		var baseSetting = CreateOrLoadBaseClip("Goblin_Base_SettingUp");
		var baseThrow = CreateOrLoadBaseClip("Goblin_Base_Throw");
		var baseDeath = CreateOrLoadBaseClip("Goblin_Base_Death");

		// Layer & StateMachine
		var layer = controller.layers[0];
		var sm = layer.stateMachine;

		// Distinct locomotion states
		var stIdle = sm.AddState("Idle");
		stIdle.motion = baseIdle;

		var stWalk = sm.AddState("Walk");
		stWalk.motion = baseWalk;

		var stJog = sm.AddState("Jog");
		stJog.motion = baseJog;

		var stAttack = sm.AddState("Attack");
		stAttack.motion = baseAttack;
		stAttack.writeDefaultValues = true;

		var stSetting = sm.AddState("SettingUp");
		stSetting.motion = baseSetting;
		stSetting.writeDefaultValues = true;

		var stThrow = sm.AddState("Throw");
		stThrow.motion = baseThrow;
		stThrow.writeDefaultValues = true;

		var stDeath = sm.AddState("Death");
		stDeath.motion = baseDeath;
		stDeath.writeDefaultValues = true;

		sm.defaultState = stIdle;

		// Idle -> Walk when !IsIdle && !IsJogging
		var idleToWalk = stIdle.AddTransition(stWalk);
		idleToWalk.hasExitTime = false;
		idleToWalk.duration = 0.05f;
		idleToWalk.AddCondition(AnimatorConditionMode.IfNot, 0, "IsIdle");
		idleToWalk.AddCondition(AnimatorConditionMode.IfNot, 0, "IsJogging");

		// Idle -> Jog when !IsIdle && IsJogging
		var idleToJog = stIdle.AddTransition(stJog);
		idleToJog.hasExitTime = false;
		idleToJog.duration = 0.05f;
		idleToJog.AddCondition(AnimatorConditionMode.IfNot, 0, "IsIdle");
		idleToJog.AddCondition(AnimatorConditionMode.If, 0, "IsJogging");

		// Walk -> Idle when IsIdle
		var walkToIdle = stWalk.AddTransition(stIdle);
		walkToIdle.hasExitTime = false;
		walkToIdle.duration = 0.05f;
		walkToIdle.AddCondition(AnimatorConditionMode.If, 0, "IsIdle");

		// Jog -> Idle when IsIdle
		var jogToIdle = stJog.AddTransition(stIdle);
		jogToIdle.hasExitTime = false;
		jogToIdle.duration = 0.05f;
		jogToIdle.AddCondition(AnimatorConditionMode.If, 0, "IsIdle");

		// Walk -> Jog when !IsIdle && IsJogging
		var walkToJog = stWalk.AddTransition(stJog);
		walkToJog.hasExitTime = false;
		walkToJog.duration = 0.05f;
		walkToJog.AddCondition(AnimatorConditionMode.IfNot, 0, "IsIdle");
		walkToJog.AddCondition(AnimatorConditionMode.If, 0, "IsJogging");

		// Jog -> Walk when !IsIdle && !IsJogging
		var jogToWalk = stJog.AddTransition(stWalk);
		jogToWalk.hasExitTime = false;
		jogToWalk.duration = 0.05f;
		jogToWalk.AddCondition(AnimatorConditionMode.IfNot, 0, "IsIdle");
		jogToWalk.AddCondition(AnimatorConditionMode.IfNot, 0, "IsJogging");

		// AnyState -> Attack (trigger)
		var anyToAttack = sm.AddAnyStateTransition(stAttack);
		anyToAttack.hasExitTime = false;
		anyToAttack.duration = 0.05f;
		anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");

		// AnyState -> Throw (trigger)
		var anyToThrow = sm.AddAnyStateTransition(stThrow);
		anyToThrow.hasExitTime = false;
		anyToThrow.duration = 0.05f;
		anyToThrow.AddCondition(AnimatorConditionMode.If, 0, "Throw");

		// AnyState -> SettingUp (bool)
		var anyToSetting = sm.AddAnyStateTransition(stSetting);
		anyToSetting.hasExitTime = false;
		anyToSetting.duration = 0.05f;
		anyToSetting.AddCondition(AnimatorConditionMode.If, 0, "IsSettingUp");

		// SettingUp -> Idle (IsSettingUp == false, with exit time)
		var settingToIdle = stSetting.AddTransition(stIdle);
		settingToIdle.hasExitTime = true;
		settingToIdle.exitTime = 0.95f;
		settingToIdle.duration = 0.05f;
		settingToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsSettingUp");

		// Attack -> Idle (exit time)
		var attackToIdle = stAttack.AddTransition(stIdle);
		attackToIdle.hasExitTime = true;
		attackToIdle.exitTime = 0.95f;
		attackToIdle.duration = 0.05f;

		// Throw -> Idle (exit time)
		var throwToIdle = stThrow.AddTransition(stIdle);
		throwToIdle.hasExitTime = true;
		throwToIdle.exitTime = 0.95f;
		throwToIdle.duration = 0.05f;

		// AnyState -> Death (IsDead)
		var anyToDeath = sm.AddAnyStateTransition(stDeath);
		anyToDeath.hasExitTime = false;
		anyToDeath.duration = 0.05f;
		anyToDeath.canTransitionToSelf = false;
		anyToDeath.AddCondition(AnimatorConditionMode.If, 0, "IsDead");
	}

	private static void AddParam(AnimatorController controller, AnimatorControllerParameterType type, string name)
	{
		if (controller.parameters.All(p => p.name != name))
		{
			controller.AddParameter(new AnimatorControllerParameter { name = name, type = type });
		}
	}

	private static AnimationClip CreateOrLoadBaseClip(string clipName)
	{
		var path = BaseClipsFolder + "/" + clipName + ".anim";
		var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
		if (clip != null) return clip;
		clip = new AnimationClip { name = clipName };
		AssetDatabase.CreateAsset(clip, path);
		return clip;
	}

	// --- Overrides ---
	private static void CreateOverride(string overrideName, RuntimeAnimatorController baseController, Dictionary<string, AnimationClip> map)
	{
		var path = Path.Combine(OutputFolder, overrideName + ".overrideController").Replace("\\", "/");
		var aoc = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);
		if (aoc == null)
		{
			aoc = new AnimatorOverrideController();
			AssetDatabase.CreateAsset(aoc, path);
		}
		aoc.runtimeAnimatorController = baseController;

		var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
		aoc.GetOverrides(overrides);
		for (int i = 0; i < overrides.Count; i++)
		{
			var baseClip = overrides[i].Key;
			var replacement = ResolveReplacement(baseClip, map);
			overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(baseClip, replacement);
		}
		aoc.ApplyOverrides(overrides);
		EditorUtility.SetDirty(aoc);
	}

	// --- Concrete controllers (no overrides) ---
	private static void CreateOrUpdateConcreteController(string fileName, Dictionary<string, AnimationClip> map)
	{
		var path = Path.Combine(OutputFolder, fileName).Replace("\\", "/");
		var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
		if (controller == null)
		{
			controller = AnimatorController.CreateAnimatorControllerAtPath(path);
			SetupBaseController(controller);
		}
		ApplyMapToController(controller, map);
		EditorUtility.SetDirty(controller);
	}

	private static void ApplyMapToController(AnimatorController controller, Dictionary<string, AnimationClip> map)
	{
		if (controller == null || map == null) return;
		var sm = controller.layers[0].stateMachine;
		// Helper to set state clip if provided
		void Set(string stateName, string key)
		{
			if (!map.TryGetValue(key, out var clip) || clip == null) return;
			var st = FindState(sm, stateName);
			if (st != null) st.motion = clip;
		}
		Set("Idle", "Idle");
		Set("Walk", "Walk");
		Set("Jog", "Jog");
		Set("Attack", "Attack");
		Set("SettingUp", "SettingUp");
		Set("Throw", "Throw");
		Set("Death", "Death");
	}

	private static AnimatorState FindState(AnimatorStateMachine sm, string name)
	{
		foreach (var child in sm.states)
		{
			if (child.state != null && child.state.name == name) return child.state;
		}
		return null;
	}

	private static AnimationClip ResolveReplacement(AnimationClip baseClip, Dictionary<string, AnimationClip> map)
	{
		if (baseClip == null) return null;
		var name = baseClip.name.ToLowerInvariant();
		// Base clip keys we generated
		if (name.Contains("idle") && map.TryGetValue("Idle", out var idle)) return idle;
		if (name.Contains("walk") && map.TryGetValue("Walk", out var walk)) return walk;
		if (name.Contains("jog") && map.TryGetValue("Jog", out var jog)) return jog;
		if (name.Contains("attack") && map.TryGetValue("Attack", out var attack)) return attack;
		if (name.Contains("settingup") && map.TryGetValue("SettingUp", out var setting)) return setting;
		if (name.Contains("throw") && map.TryGetValue("Throw", out var thr)) return thr;
		if (name.Contains("death") && map.TryGetValue("Death", out var death)) return death;
		return baseClip; // fallback to base
	}

	// --- Mapping per goblin ---
	private static Dictionary<string, AnimationClip> MapDaggerGoblin(List<AnimationClip> pool)
	{
		return new Dictionary<string, AnimationClip>
		{
			{"Idle", FindBest(pool, new[]{"goblin","dagger","assassin","asassin","classic"}, new[]{"idle"})},
			{"Walk", FindBest(pool, new[]{"goblin","assassin","asassin","classic"}, new[]{"walk"})},
			{"Jog", FindBest(pool, new[]{"goblin","dagger","assassin","asassin","classic"}, new[]{"jog","run"})},
			{"Attack", FindBest(pool, new[]{"goblin","dagger","assassin","asassin","classic"}, new[]{"attack","slash","stab"})},
			{"SettingUp", null},
			{"Throw", null},
			{"Death", FindBest(pool, new[]{"goblin","dagger","assassin","asassin","classic"}, new[]{"death","die"})},
		};
	}

	private static Dictionary<string, AnimationClip> MapDaggerGoblinBomber(List<AnimationClip> pool)
	{
		return new Dictionary<string, AnimationClip>
		{
			{"Idle", FindBest(pool, new[]{"goblin","bomber","dagger","assassin","asassin","bomber_goblin"}, new[]{"idle"})},
			{"Walk", FindBest(pool, new[]{"goblin","bomber","bomber_goblin"}, new[]{"walk"})},
			{"Jog", FindBest(pool, new[]{"goblin","bomber","dagger","assassin","asassin","bomber_goblin"}, new[]{"jog","run"})},
			{"Attack", FindBest(pool, new[]{"goblin","bomber","dagger","assassin","asassin","bomber_goblin"}, new[]{"attack"})},
			{"SettingUp", null},
			{"Throw", null},
			{"Death", FindBest(pool, new[]{"goblin","bomber","dagger","assassin","asassin","bomber_goblin"}, new[]{"death","die"})},
		};
	}

	private static Dictionary<string, AnimationClip> MapTrapperGoblin(List<AnimationClip> pool)
	{
		return new Dictionary<string, AnimationClip>
		{
			{"Idle", FindBest(pool, new[]{"goblin","trapper","trapper_goblin"}, new[]{"idle"})},
			{"Walk", FindBest(pool, new[]{"goblin","trapper","trapper_goblin"}, new[]{"walk"})},
			{"Jog", null},
			{"Attack", null},
			{"SettingUp", FindBest(pool, new[]{"goblin","trapper","trapper_goblin"}, new[]{"setup","setting","trap","place"})},
			{"Throw", null},
			{"Death", FindBest(pool, new[]{"goblin","trapper","trapper_goblin"}, new[]{"death","die"})},
		};
	}

	private static Dictionary<string, AnimationClip> MapTrapperGoblinBombs(List<AnimationClip> pool)
	{
		return new Dictionary<string, AnimationClip>
		{
			{"Idle", FindBest(pool, new[]{"goblin","trapper","bomb","trapper_bomber","trapper_bomber goblin"}, new[]{"idle"})},
			{"Walk", FindBest(pool, new[]{"goblin","trapper","bomb","trapper_bomber","trapper_bomber goblin"}, new[]{"walk"})},
			{"Jog", null},
			{"Attack", null},
			{"SettingUp", FindBest(pool, new[]{"goblin","trapper","bomb","trapper_bomber","trapper_bomber goblin"}, new[]{"setup","setting","trap","place"})},
			{"Throw", FindBest(pool, new[]{"goblin","trapper","bomb","trapper_bomber","trapper_bomber goblin"}, new[]{"throw"})},
			{"Death", FindBest(pool, new[]{"goblin","trapper","bomb","trapper_bomber","trapper_bomber goblin"}, new[]{"death","die"})},
		};
	}

	// --- Utilities ---
	private static List<AnimationClip> LoadAllClips()
	{
		// Filter to only existing folders to avoid 'Folder not found' errors
		var validFolders = SearchFolders.Where(AssetDatabase.IsValidFolder).ToArray();
		if (validFolders == null || validFolders.Length == 0)
		{
			validFolders = new[] { "Assets" };
		}
		var ids = AssetDatabase.FindAssets("t:AnimationClip", validFolders);
		var clips = new List<AnimationClip>(ids.Length);
		foreach (var id in ids)
		{
			var path = AssetDatabase.GUIDToAssetPath(id);
			var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
			if (clip != null) clips.Add(clip);
		}
		return clips;
	}

	private static AnimationClip FindBest(IEnumerable<AnimationClip> pool, string[] typeKeywords, string[] actionKeywords)
	{
		AnimationClip best = null;
		int bestScore = -1;
		foreach (var clip in pool)
		{
			var name = clip.name.ToLowerInvariant();
			int score = 0;
			foreach (var tk in typeKeywords)
			{
				if (!string.IsNullOrEmpty(tk) && name.Contains(tk)) score += 2;
			}
			foreach (var ak in actionKeywords)
			{
				if (!string.IsNullOrEmpty(ak) && name.Contains(ak)) score += 3;
			}
			// Generic goblin keyword (bonus)
			if (name.Contains("goblin")) score += 1;

			if (score > bestScore)
			{
				best = clip;
				bestScore = score;
			}
		}
		return best; // may be null, override keeps base placeholders
	}

	private static void EnsureFolders()
	{
		CreateFolderIfMissing("Assets", "Animations");
		CreateFolderIfMissing("Assets/Animations", "Goblins");
		CreateFolderIfMissing("Assets/Animations/Goblins", "_Generated");
		CreateFolderIfMissing(OutputFolder, "BaseClips");
	}

	private static void CreateFolderIfMissing(string parent, string child)
	{
		var path = Path.Combine(parent, child).Replace("\\", "/");
		if (!AssetDatabase.IsValidFolder(path))
		{
			AssetDatabase.CreateFolder(parent, child);
		}
	}
}
