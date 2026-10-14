using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace OneLittleStep.Tests
{
    /// <summary>Drives the real uGUI by visible labels, the way a player would.</summary>
    public static class UiDriver
    {
        public static Bootstrap Boot => Object.FindAnyObjectByType<Bootstrap>();
        public static AppController App => Boot.App;
        public static GameModel Model => Boot.Model;

        public static Button FindButton(string label)
        {
            return Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                .Where(b => b.isActiveAndEnabled)
                .FirstOrDefault(b => b.GetComponentsInChildren<Text>(true).Any(t => t.text == label));
        }

        public static IEnumerator Click(string label)
        {
            var b = FindButton(label);
            Assert.IsNotNull(b, $"No visible button '{label}' on {App.Current}/{App.CurrentOverlay}");
            Assert.IsTrue(b.interactable, $"Button '{label}' is disabled");
            b.onClick.Invoke();
            yield return null;
            yield return null;
        }

        public static IEnumerator ClickNamed(string objectName)
        {
            var b = Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                .FirstOrDefault(x => x.isActiveAndEnabled && x.name == objectName);
            Assert.IsNotNull(b, $"No visible '{objectName}' on {App.Current}");
            b.onClick.Invoke();
            yield return null;
            yield return null;
        }

        public static bool IsVisible(string objectName) =>
            Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Any(x => x.isActiveAndEnabled && x.name == objectName);

        public static bool IsInteractable(string label)
        {
            var b = FindButton(label);
            return b != null && b.interactable;
        }

        public static void Type(string placeholder, string text)
        {
            var field = Object.FindObjectsByType<InputField>(FindObjectsSortMode.None)
                .Where(f => f.isActiveAndEnabled)
                .FirstOrDefault(f => f.placeholder is Text t && t.text == placeholder);
            Assert.IsNotNull(field, $"No visible input '{placeholder}'");
            field.text = text;
        }
    }
}
