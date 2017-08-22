﻿using Merlin.API;
using System.Linq;

namespace Merlin.Profiles.Gatherer
{
    public partial class Gatherer
    {
        #region Methods

        public bool HandleAttackers()
        {
            if (_localPlayerCharacterView.IsUnderAttack(out FightingObjectView attacker))
            {
                _localPlayerCharacterView.CreateTextEffect("[Attacked]");

                _state.Fire(Trigger.EncounteredAttacker);
                return true;
            }

            return false;
        }

        public void Fight()
        {
            var player = _localPlayerCharacterView;

            if (player.IsMounted)
            {
                player.MountOrDismount();
                return;
            }

            var spells = player.GetSpells().Ready()
                                .Ignore("ESCAPE_DUNGEON").Ignore("PLAYER_COUPDEGRACE")
                                .Ignore("AMBUSH");

            var attackTarget = player.GetAttackTarget();

            if (attackTarget != null && !attackTarget.IsDead())
            {
                var selfBuffSpells = spells.Target(gz.SpellTarget.Self).Category(gz.SpellCategory.Buff);
                if (selfBuffSpells.Any() && !player.IsCastingSpell())
                {
                    player.CreateTextEffect("[Casting Buff Spell]");
                    player.CastOnSelf(selfBuffSpells.FirstOrDefault().SpellSlot);
                    return;
                }

                var selfDamageSpells = spells.Target(gz.SpellTarget.Self).Category(gz.SpellCategory.Damage);
                if (selfDamageSpells.Any() && !player.IsCastingSpell())
                {
                    player.CreateTextEffect("[Casting Damage Spell]");
                    player.CastOnSelf(selfDamageSpells.FirstOrDefault().SpellSlot);
                    return;
                }

                var groundCCSpells = spells.Target(gz.SpellTarget.Ground).Category(gz.SpellCategory.CrowdControl);
                if (groundCCSpells.Any())
                {
                    player.CreateTextEffect("[Casting Ground Spell]");
                    player.CastAt(groundCCSpells.FirstOrDefault().SpellSlot, attackTarget.transform.position);
                    return;
                }

                var selfCCSpells = spells.Target(gz.SpellTarget.Self).Category(gz.SpellCategory.CrowdControl);
                if (selfCCSpells.Any())
                {
                    player.CreateTextEffect("[Casting Self Spell]");
                    player.CastOnSelf(selfCCSpells.FirstOrDefault().SpellSlot);
                    return;
                }

                var enemyDamageSpells = spells.Target(gz.SpellTarget.Enemy).Category(gz.SpellCategory.Damage);
                if (enemyDamageSpells.Any() && !player.IsCastingSpell())
                {
                    player.CreateTextEffect("[Casting Damage Spell]");
                    player.CastOn(enemyDamageSpells.FirstOrDefault().SpellSlot, player.GetAttackTarget());
                    return;
                }

                // TODO: If buffed, don't use channeled spells.

                /*
				var enemyDamageSpells = spells.Target(gs.SpellTarget.Enemy).Category(gs.SpellCategory.Damage);
				if (enemyDamageSpells.Any() && !player.IsCastingSpell())
				{
					player.CreateTextEffect("[Casting Damage Spell]");
					player.CastOn(enemyDamageSpells.FirstOrDefault().SpellSlot, player.GetAttackTarget());
					return;
				}
				*/

                /*
				var selfDamageSpells = spells.Target(gs.SpellTarget.Self).Category(gs.SpellCategory.Damage);
				if (selfDamageSpells.Any())
				{
				}

				*/
            }

            if (player.IsUnderAttack(out FightingObjectView attacker))
            {
                player.SetSelectedObject(attacker);
                player.AttackSelectedObject();
                return;
            }

            if (player.IsCasting())
                return;

            if (player.GetHealth() < (player.GetMaxHealth() * _minimumHealthForGathering))
            {
                Core.Log($"[Regen Health - {(player.GetHealth()/player.GetMaxHealth()).ToString("P2")}]");
                var healSpell = spells.Target(gz.SpellTarget.Self).Category(gz.SpellCategory.Heal);

                if (healSpell.Any())
                    player.CastOnSelf(healSpell.FirstOrDefault().SpellSlot);

                return;
            }

            _currentTarget = null;
            _harvestPathingRequest = null;

            Core.Log("[Eliminated]");
            _state.Fire(Trigger.EliminatedAttacker);
        }

        #endregion Methods
    }
}