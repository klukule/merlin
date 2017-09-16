using Merlin.API;
using Merlin.API.Direct;
using System.Linq;

namespace Merlin.Profiles.Gatherer
{
    public sealed partial class Gatherer
    {
        public void Fight()
        {
            LocalPlayerCharacter player = _localPlayerCharacterView.GetLocalPlayerCharacter();

            if (_localPlayerCharacterView.IsMounted)
            {
                _localPlayerCharacterView.MountOrDismount();
                return;
            }

            var spells = player.GetSpellSlotsIndexed().Ready(_localPlayerCharacterView).Ignore("ESCAPE_DUNGEON").Ignore("PLAYER_COUPDEGRACE").Ignore("AMBUSH");

            #region [Use all spells] - dTormentedSoul // - dTormentedSoul
            FightingObjectView attackTarget = _localPlayerCharacterView.GetAttackTarget();
            if (_localPlayerCharacterView.IsCasting()) // - dTormentedSoul
            {
                //_localPlayerCharacterView.CreateTextEffect("[Is casting]");
                return;
            }
            if (attackTarget != null && !attackTarget.IsDead())
            {
                LocalPlayerCharacter localPlayer = _localPlayerCharacterView.LocalPlayerCharacter;

                //_localPlayerCharacterView.CreateTextEffect("[Use all spells] | " + (attackTarget != null ? attackTarget.name : "none"));
                if (spells.Any() && !_localPlayerCharacterView.IsCasting())
                {
                    foreach (SpellSlot sp in spells)
                    {
                        try
                        {
                            _localPlayerCharacterView.CastOnSelf(sp.Slot);
                        }
                        catch { }
                        try
                        {
                            _localPlayerCharacterView.CastOn(sp.Slot, attackTarget);
                        }
                        catch { }
                        try
                        {
                            _localPlayerCharacterView.CastAt(sp.Slot, attackTarget.GetPosition());
                        }
                        catch { }
                    }
                    if (!_localPlayerCharacterView.IsCasting())
                    {
                        _localPlayerCharacterView.CastOnSelf(spells.FirstOrDefault().Slot);
                        _localPlayerCharacterView.CastOn(spells.FirstOrDefault().Slot, attackTarget);
                        _localPlayerCharacterView.CastAt(spells.FirstOrDefault().Slot, attackTarget.GetPosition());
                    }
                    return;
                }
            }
            Profile.UpdateDelay = System.TimeSpan.FromSeconds(0.1d); // - dTormentedSoul
            #endregion [Use all spells] - dTormentedSoul // - dTormentedSoul


            if (_localPlayerCharacterView.IsUnderAttack(out FightingObjectView attacker))
            {
                _localPlayerCharacterView.SetSelectedObject(attacker);
                _localPlayerCharacterView.AttackSelectedObject();
                return;
            }

            #region [health check] // - dTormentedSoul
            if (player.GetHealth().GetValue() < (player.GetHealth().GetMaximum() * _minimumHealthForGathering))
            {
                try 
                {

                    var healSpell = spells.Target(SpellTarget.Self).Category(SpellCategory.Heal);

                    if (healSpell.Any())
                        _localPlayerCharacterView.CastOnSelf(healSpell.FirstOrDefault().Slot);
                }
                catch { }
                _localPlayerCharacterView.CreateTextEffect("[Waiting for Health]");
                return;
            }
            if (player.GetEnergy().GetValue() < (player.GetEnergy().GetMaximum() * 0.4f))
            {
                _localPlayerCharacterView.CreateTextEffect("[Waiting for Energy]");
                return;
            }
            #endregion [health check] // - dTormentedSoul

            _currentTarget = null;
            _harvestPathingRequest = null;

            Core.Log("[Eliminated]");
            _state.Fire(Trigger.EliminatedAttacker);
        }
    }
}