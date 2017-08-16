using Merlin.API.Direct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;

using YinYang.CodeProject.Projects.SimplePathfinding.Helpers;
using YinYang.CodeProject.Projects.SimplePathfinding.PathFinders.AStar;

namespace Merlin
{
    public static class LocalPlayerCharacterViewExtensions
    {
        private static MethodInfo _doActionStaticObjectInteraction;

        public static LocalPlayerCharacter GetCharacter(this LocalPlayerCharacterView view) => view.LocalPlayerCharacter;
        public static bool IsUnderAttack(this LocalPlayerCharacterView view, out FightingObjectView attacker)
        {
            var entities = GameManager.GetInstance().GetEntities<MobView>((entity) => {
                var target = ((FightingObjectView)entity).GetAttackTarget();

                if (target != null && target == view)
                    return true;

                return false;
            });

            attacker = entities.FirstOrDefault();

            return attacker != default(FightingObjectView);

        }
        public static float GetWeightPercentage(this LocalPlayerCharacterView view)
        {
            return view.GetCharacter().GetLoad() / view.GetCharacter().GetMaxLoad() * 100.0f * 2f;
        }

        public static void Interact(this LocalPlayerCharacterView instance, WorldObjectView target)
        {
            _doActionStaticObjectInteraction.Invoke(instance.InputHandler, new object[] { target, String.Empty });
        }
    }
}
