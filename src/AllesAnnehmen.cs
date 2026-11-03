// ---------------------------------------------------------------
//  Alles Annehmen  -  SPT 4.1.6
//
//  Ein Knopf im Nachrichtenfenster, der ALLE Anhaenge von ALLEN
//  Haendlern einsammelt und direkt ins Versteck legt. Kein
//  Uebergabefenster, kein Ziehen mit der Maus, kein Haendler-fuer-
//  Haendler-Durchklicken.
//
//  Wie das Spiel es selbst macht (nachgesehen im dekompilierten
//  Assembly-CSharp von SPT 4.1.6):
//    ChatScreen.TransferAll(dialog)
//      -> SocialNetwork.AllAttachmentsFromDialog(id, cb)   holt die
//         Nachrichten eines Gespraechs mit ihren Anhaengen vom Server
//      -> ChatScreen.ProcessRewards(...)                   sortiert
//         Aufgabengegenstaende / normale Gegenstaende / Profilboni
//      -> TransferItemsScreen.TransferAllItems()           schiebt
//         jeden Gegenstand mit QuickFindAppropriatePlace +
//         TryRunNetworkTransaction ins Versteck
//  Diese Mod macht genau dasselbe, nur ueber alle Gespraeche hinweg
//  und ohne die Fenster dazwischen.
// ---------------------------------------------------------------
using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.UI.Chat;
using HarmonyLib;
using UnityEngine;

namespace AllesAnnehmen
{
	[BepInPlugin("com.kaan.allesannehmen", "Alles Annehmen", "1.2.0")]
	public class AllesAnnehmenPlugin : BaseUnityPlugin
	{
		internal static ManualLogSource Protokoll;

		internal static ConfigEntry<bool> KnopfZeigen;
		internal static ConfigEntry<string> KnopfText;
		internal static ConfigEntry<KeyboardShortcut> Taste;
		internal static ConfigEntry<bool> ProfilBoniEinloesen;
		internal static ConfigEntry<bool> AlsGelesenMarkieren;
		internal static ConfigEntry<bool> AnhangKnopfUmbiegen;

		private void Awake()
		{
			Protokoll = Logger;

			KnopfZeigen = Config.Bind("Bedienung", "Knopf anzeigen", true,
				"Setzt einen Knopf oben ins Nachrichtenfenster. Aus: nur das Tastenkuerzel.");

			KnopfText = Config.Bind("Bedienung", "Knopfbeschriftung", "ALLE ANNEHMEN",
				"Was auf dem Knopf steht.");

			Taste = Config.Bind("Bedienung", "Tastenkuerzel", new KeyboardShortcut(KeyCode.F9),
				"Wirkt nur, solange das Nachrichtenfenster offen ist. Funktioniert auch, "
				+ "wenn der Knopf sich nicht setzen liess.");

			AnhangKnopfUmbiegen = Config.Bind("Bedienung", "Anhang-Knopf umbiegen", true,
				"Der Bueroklammer-Knopf neben jedem Haendler (Tooltip \"Alles Annehmen\") "
				+ "raeumt dann ebenfalls ALLE Haendler auf einmal ab, statt nur den einen "
				+ "und statt das Uebergabefenster zu oeffnen. Aus: Knopf verhaelt sich wie "
				+ "im unveraenderten Spiel.");

			ProfilBoniEinloesen = Config.Bind("Umfang", "Profilboni mit einloesen", true,
				"Auch Belohnungen ohne Gegenstand (Geld, Erfahrung, Haendlerruf) sofort "
				+ "annehmen, statt das Belohnungsfenster zu zeigen.");

			AlsGelesenMarkieren = Config.Bind("Umfang", "Alles als gelesen markieren", true,
				"Raeumt am Ende die gruenen Ungelesen-Zaehler weg.");

			try
			{
				new Harmony("com.kaan.allesannehmen").PatchAll(Assembly.GetExecutingAssembly());
				Logger.LogInfo("Alles Annehmen 1.2.0 bereit.");
			}
			catch (Exception e)
			{
				Logger.LogError("Alles Annehmen konnte sich nicht einhaengen: " + e);
			}
		}
	}

	/// <summary>
	/// Biegt den spieleigenen Anhang-Knopf um. ChatScreen.TransferAll
	/// holt normalerweise nur EIN Gespraech ab und oeffnet danach das
	/// Uebergabefenster - wir machen daraus "alle, direkt ins Versteck".
	/// </summary>
	[HarmonyPatch(typeof(ChatScreen), "TransferAll")]
	internal static class AnhangKnopfPatch
	{
		[HarmonyPrefix]
		private static bool Davor(ChatScreen __instance)
		{
			if (!AllesAnnehmenPlugin.AnhangKnopfUmbiegen.Value)
			{
				return true; // Originalverhalten
			}

			try
			{
				Bedienteil.Ausloesen(__instance, null);
			}
			catch (Exception e)
			{
				AllesAnnehmenPlugin.Protokoll.LogError("Anhang-Knopf: " + e);
				return true;
			}
			return false; // Original nicht mehr ausfuehren
		}
	}

	/// <summary>
	/// Haengt sich an das Oeffnen des Nachrichtenfensters. ChatScreen.Show
	/// gibt es mehrfach (einmal geerbt von EftScreen) - wir nehmen die
	/// Fassung mit fuenf Parametern, das ist die echte.
	/// </summary>
	[HarmonyPatch]
	internal static class NachrichtenfensterPatch
	{
		[HarmonyPrepare]
		private static bool Vorbereiten()
		{
			return ZielMethode() != null;
		}

		[HarmonyTargetMethod]
		private static MethodBase ZielMethode()
		{
			return AccessTools.GetDeclaredMethods(typeof(ChatScreen))
				.FirstOrDefault(m => m.Name == "Show" && m.GetParameters().Length == 5);
		}

		[HarmonyPostfix]
		private static void Danach(ChatScreen __instance)
		{
			try
			{
				Bedienteil.Anbauen(__instance);
			}
			catch (Exception e)
			{
				AllesAnnehmenPlugin.Protokoll.LogError("Knopf/Taste liessen sich nicht anbauen: " + e);
			}
		}
	}
}
