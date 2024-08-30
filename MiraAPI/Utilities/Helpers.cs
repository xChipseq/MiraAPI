﻿using MiraAPI.Roles;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Utilities;

public static class Helpers
{
    public static readonly ContactFilter2D Filter = ContactFilter2D.CreateLegacyFilter(Constants.NotShipMask, float.MinValue, float.MaxValue);
    public static PlainShipRoom GetRoom(Vector3 pos)
    {
        return ShipStatus.Instance.AllRooms.ToList().Find(room => room.roomArea.OverlapPoint(pos));
    }

    public static List<DeadBody> GetNearestDeadBodies(Vector2 source, float radius)
    {
        var results = new Il2CppSystem.Collections.Generic.List<Collider2D>();
        Physics2D.OverlapCircle(source, radius, Filter, results);
        return results.ToArray()
            .Where(collider2D => collider2D.CompareTag("DeadBody"))
            .Select(collider2D => collider2D.GetComponent<DeadBody>()).ToList();
    }

    public static List<T> GetNearestObjectsOfType<T>(Vector2 source, float radius, string? colliderTag = null) where T : Component
    {
        var results = new Il2CppSystem.Collections.Generic.List<Collider2D>();
        Physics2D.OverlapCircle(source, radius, Filter, results);
        return results.ToArray()
            .Where(collider2D => colliderTag == null || collider2D.CompareTag(colliderTag))
            .Select(collider2D => collider2D.GetComponent<T>()).ToList();
    }

    public static List<PlayerControl> GetClosestPlayersInCircle(Vector2 source, float radius, bool ignoreColliders = true)
    {
        var newList = GetNearestObjectsOfType<PlayerControl>(source, radius);

        if (ignoreColliders)
        {
            var filteredList = new List<PlayerControl>();
            foreach (var player in newList)
            {
                var vector = player.GetTruePosition() - source;
                var magnitude = vector.magnitude;
                if (!PhysicsHelpers.AnyNonTriggersBetween(source, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                {
                    filteredList.Add(player);
                }
            }

            return filteredList;
        }

        return newList;
    }
    public static List<PlayerControl> GetClosestPlayers(PlayerControl source, float distance = 2f, bool ignoreColliders = true, bool ignoreSource = true)
    {
        if (!ShipStatus.Instance)
        {
            return [];
        }

        var myPos = source.GetTruePosition();
        var players = GetClosestPlayers(myPos, distance, ignoreColliders);

        return ignoreSource ? players.Where(plr => plr.PlayerId != source.PlayerId).ToList() : players;
    }

    public static List<PlayerControl> GetClosestPlayers(Vector2 source, float distance = 2f, bool ignoreColliders = true)
    {
        if (!ShipStatus.Instance)
        {
            return [];
        }

        List<PlayerControl> outputList = [];
        outputList.Clear();
        var allPlayers = GameData.Instance.AllPlayers.ToArray().Select(x=>x.Object);

        foreach (var playerControl in allPlayers)
        {
            if (!playerControl || !playerControl.Collider.enabled)
            {
                continue;
            }

            var vector = playerControl.GetTruePosition() - source;
            var magnitude = vector.magnitude;
            if (magnitude <= distance && (ignoreColliders || !PhysicsHelpers.AnyNonTriggersBetween(source, vector.normalized, magnitude, Constants.ShipAndObjectsMask)))
            {
                outputList.Add(playerControl);
            }
        }

        outputList.Sort(delegate (PlayerControl a, PlayerControl b)
        {
            var magnitude2 = (a.GetTruePosition() - source).magnitude;
            var magnitude3 = (b.GetTruePosition() - source).magnitude;
            if (magnitude2 > magnitude3)
            {
                return 1;
            }
            if (magnitude2 < magnitude3)
            {
                return -1;
            }
            return 0;
        });
        return outputList;
    }

    public static TextMeshPro CreateTextLabel(string name, Transform parent,
AspectPosition.EdgeAlignments alignment, Vector3 distance, float fontSize = 2f,
TextAlignmentOptions textAlignment = TextAlignmentOptions.Center)
    {
        var textObj = new GameObject(name)
        {
            transform =
            {
                parent = parent
            },
            layer = LayerMask.NameToLayer("UI")
        };

        var textMeshPro = textObj.AddComponent<TextMeshPro>();
        textMeshPro.fontSize = fontSize;
        textMeshPro.alignment = textAlignment;
        textMeshPro.font = HudManager.Instance.TaskPanel.taskText.font;
        textMeshPro.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;

        var aspectPosition = textObj.AddComponent<AspectPosition>();
        aspectPosition.Alignment = alignment;
        aspectPosition.DistanceFromEdge = distance;
        aspectPosition.AdjustPosition();

        return textMeshPro;
    }

    public static DeadBody GetBodyById(byte id)
    {
        return Object.FindObjectsOfType<DeadBody>().FirstOrDefault(body => body.ParentId == id);
    }

    public static string GetSuffix(MiraNumberSuffixes suffix)
    {
        return suffix switch
        {
            MiraNumberSuffixes.None => string.Empty,
            MiraNumberSuffixes.Multiplier => "x",
            MiraNumberSuffixes.Seconds => "s",
            MiraNumberSuffixes.Percent => "%",
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Creates a string builder for the Role Tab.
    /// </summary>
    /// <param name="role">The ICustomRole object.</param>
    /// <returns>A StringBuilder.</returns>
    public static StringBuilder CreateForRole(ICustomRole role)
    {
        var taskStringBuilder = new StringBuilder();
        taskStringBuilder.AppendLine($"{role.RoleColor.ToTextColor()}You are a <b>{role.RoleName}.</b></color>");
        taskStringBuilder.Append("<size=70%>");
        taskStringBuilder.AppendLine($"{role.RoleLongDescription}");
        return taskStringBuilder;
    }
}
