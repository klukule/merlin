using Merlin.API;
using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Merlin.Profiles.Gatherer
{
    public partial class Gatherer : Profile
    {
        #region Fields

        static string _prefsIdentifier = "gath_";

        bool _isRunning;
        StateMachine<State, Trigger> _state;
        Dictionary<SimulationObjectView, Blacklisted> _blacklist;
        Dictionary<Vector3, GatherInformation> _gatheredSpots;
        List<MobView> _keepers;
        string _selectedGatherCluster;
        bool _knockedDown;

        //Savable (init defaults in OnStart)
        bool _allowMobHunting;
        bool _skipUnrestrictedPvPZones;
        bool _skipKeeperPacks;
        bool _allowSiegeCampTreasure;
        float _keeperSkipRange;
        float _minimumHealthForGathering;
        int _selectedTownClusterIndex;
        int _selectedMininumTierIndex;
        Dictionary<GatherInformation, bool> _gatherInformations;

        #endregion Fields

        #region Properties and Events

        public override string Name => "Gatherer";

        public string[] TownClusterNames { get { return Enum.GetNames(typeof(TownClusterName)).Select(n => n.Replace("_", " ")).ToArray(); } }

        public string[] TierNames { get { return Enum.GetNames(typeof(Tier)).ToArray(); } }

        public string SelectedTownCluster { get { return TownClusterNames[_selectedTownClusterIndex]; } }

        public Tier SelectedMinimumTier { get { return (Tier)Enum.Parse(typeof(Tier), TierNames[_selectedMininumTierIndex]); } }

        #endregion Properties and Events

        #region Methods

        protected override void OnStart()
        {
            _blacklist = new Dictionary<SimulationObjectView, Blacklisted>();
            _gatheredSpots = new Dictionary<Vector3, GatherInformation>();

            _allowMobHunting = bool.Parse(PlayerPrefs.GetString($"{_prefsIdentifier}{nameof(_allowMobHunting)}", bool.FalseString));
            _skipUnrestrictedPvPZones = bool.Parse(PlayerPrefs.GetString($"{_prefsIdentifier}{nameof(_skipUnrestrictedPvPZones)}", bool.TrueString));
            _skipKeeperPacks = bool.Parse(PlayerPrefs.GetString($"{_prefsIdentifier}{nameof(_skipKeeperPacks)}", bool.TrueString));
            _allowSiegeCampTreasure = bool.Parse(PlayerPrefs.GetString($"{_prefsIdentifier}{nameof(_allowSiegeCampTreasure)}", bool.TrueString));
            _keeperSkipRange = PlayerPrefs.GetFloat($"{_prefsIdentifier}{nameof(_keeperSkipRange)}", 22);
            _minimumHealthForGathering = PlayerPrefs.GetFloat($"{_prefsIdentifier}{nameof(_minimumHealthForGathering)}", 0.8f);
            _selectedTownClusterIndex = PlayerPrefs.GetInt($"{_prefsIdentifier}{nameof(_selectedTownClusterIndex)}", 0);
            _selectedMininumTierIndex = PlayerPrefs.GetInt($"{_prefsIdentifier}{nameof(_selectedMininumTierIndex)}", 0);
            _gatherInformations = new Dictionary<GatherInformation, bool>();
            foreach (var resourceType in Enum.GetValues(typeof(ResourceType)).Cast<ResourceType>())
                foreach (var tier in Enum.GetValues(typeof(Tier)).Cast<Tier>())
                    foreach (var enchantment in Enum.GetValues(typeof(EnchantmentLevel)).Cast<EnchantmentLevel>())
                    {
                        if (tier < Tier.IV && enchantment != EnchantmentLevel.White)
                            continue;

                        var info = new GatherInformation(resourceType, tier, enchantment);
                        var val = bool.Parse(PlayerPrefs.GetString($"{_prefsIdentifier}{info.ToString()}", (tier >= Tier.II).ToString()));
                        _gatherInformations.Add(info, val);
                    }

            _state = new StateMachine<State, Trigger>(State.Search);
            _state.Configure(State.Search)
                .Permit(Trigger.StartSiegeCampTreasure, State.SiegeCampTreasure)
                .Permit(Trigger.StartTravelling, State.Travel)
                .Permit(Trigger.EncounteredAttacker, State.Combat)
                .Permit(Trigger.DiscoveredResource, State.Harvest)
                .Permit(Trigger.Overweight, State.Bank);

            _state.Configure(State.Combat)
                .Permit(Trigger.Restart, State.Search)
                .Permit(Trigger.EliminatedAttacker, State.Search);

            _state.Configure(State.Harvest)
                .Permit(Trigger.Restart, State.Search)
                .Permit(Trigger.DepletedResource, State.Search)
                .Permit(Trigger.EncounteredAttacker, State.Combat);

            _state.Configure(State.Travel)
                .Permit(Trigger.Restart, State.Search)
                .Permit(Trigger.TravellingDone, State.Search);

            _state.Configure(State.Bank)
                .Permit(Trigger.Restart, State.Search)
                .Permit(Trigger.StartTravelling, State.Travel)
                .Permit(Trigger.BankDone, State.Travel);

            _state.Configure(State.SiegeCampTreasure)
                .Permit(Trigger.Restart, State.Search)
                .Permit(Trigger.OnSiegeCampTreasureDone, State.Search);
        }

        protected override void OnStop()
        {
            PlayerPrefs.SetString($"{_prefsIdentifier}{nameof(_allowMobHunting)}", _allowMobHunting.ToString());
            PlayerPrefs.SetString($"{_prefsIdentifier}{nameof(_skipUnrestrictedPvPZones)}", _skipUnrestrictedPvPZones.ToString());
            PlayerPrefs.SetString($"{_prefsIdentifier}{nameof(_skipKeeperPacks)}", _skipKeeperPacks.ToString());
            PlayerPrefs.SetString($"{_prefsIdentifier}{nameof(_allowSiegeCampTreasure)}", _allowSiegeCampTreasure.ToString());
            PlayerPrefs.SetFloat($"{_prefsIdentifier}{nameof(_keeperSkipRange)}", _keeperSkipRange);
            PlayerPrefs.SetFloat($"{_prefsIdentifier}{nameof(_minimumHealthForGathering)}", _minimumHealthForGathering);
            PlayerPrefs.SetInt($"{_prefsIdentifier}{nameof(_selectedTownClusterIndex)}", _selectedTownClusterIndex);
            PlayerPrefs.SetInt($"{_prefsIdentifier}{nameof(_selectedMininumTierIndex)}", _selectedMininumTierIndex);
            foreach (var kvp in _gatherInformations)
                PlayerPrefs.SetString($"{_prefsIdentifier}{kvp.Key.ToString()}", kvp.Value.ToString());

            _state = null;

            _blacklist.Clear();
            _blacklist = null;
        }

        protected override void OnUpdate()
        {
            if (_autoUpdate)
            {
                Client.Zoom = _zoom;
                Client.GlobalFog = _globalFog;
            }

            if (!_isRunning)
                return;

            if (_blacklist.Count > 0)
            {
                var whitelist = new List<SimulationObjectView>();

                foreach (var blacklisted in _blacklist.Values)
                {
                    if (DateTime.Now >= blacklisted.Timestamp)
                        whitelist.Add(blacklisted.Target);
                }

                foreach (var target in whitelist)
                    _blacklist.Remove(target);
            }

            try
            {
                _keepers = _client.GetEntities<MobView>(mob => !mob.IsDead() && mob.MobType().ToLowerInvariant().Contains("keeper"));

                if (_knockedDown != _localPlayerCharacterView.IsKnockedDown())
                {
                    _knockedDown = _localPlayerCharacterView.IsKnockedDown();
                    if (_knockedDown)
                    {
                        Core.Log("[DEAD - Currently knocked down!]");
                        Application.CaptureScreenshot(DateTime.UtcNow.ToShortDateString() + ".png");
                        Core.Log($"[Screenshot taken to : {Application.persistentDataPath}");
                    }
                }

                switch (_state.State)
                {
                    case State.Search:
                        Search();
                        break;

                    case State.Harvest:
                        Harvest();
                        break;

                    case State.Combat:
                        Fight();
                        break;

                    case State.Travel:
                        Travel();
                        break;

                    case State.Bank:
                        Bank();
                        break;

                    case State.SiegeCampTreasure:
                        SiegeCampTreasure();
                        break;
                }
            }
            catch (Exception e)
            {
                if (_showErrors)
                    _localPlayerCharacterView.CreateTextEffect($"[Error: {e.Message}]");

                Core.Log(e);
                ResetCriticalVariables();
                _state.Fire(Trigger.Restart);
            }
        }

        private void ResetCriticalVariables()
        {
            _targetCluster = null;
            _worldPathingRequest = null;
            _siegeCampTreasureCoroutine = null;
            _currentTarget = null;
            _failedFindAttempts = 0;
            _changeGatheringPathRequest = null;
            _harvestPathingRequest = null;
            _bankEntracePathingRequest = null;
            _bankLeavePathingRequest = null;
            _bankPathingRequest = null;
            _isDepositing = false;
        }

        private void Blacklist(SimulationObjectView target, TimeSpan duration)
        {
            _blacklist[target] = new Blacklisted()
            {
                Target = target,
                Timestamp = DateTime.Now + duration,
            };
        }

        #endregion Methods
    }
}