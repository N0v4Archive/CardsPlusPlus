﻿using ModsPlus;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib.Cards;
using UnityEngine;
using UnboundLib;

namespace CardsPlusPlugin.Cards
{
    public class HotPotato : CustomEffectCard<HotPotatoEffect>
    {
        public override CardDetails Details => new CardDetails
        {
            Title       = "Hot Potato",
            Description = "Leave a trail of burning fire in your wake",
            ModName     = "Cards+",
            Art         = Assets.HotPotatoArt,
            Rarity      = CardInfo.Rarity.Uncommon,
            Theme       = CardThemeColor.CardThemeColorType.DestructiveRed
        };

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
        }
    }

    public class HotPotatoEffect : CardEffect
    {
        private float spawnDelay = 0.1f;
        private float lifetime = 0.8f;
        private Coroutine resetCoroutine;

        protected override void Start()
        {
            base.Start();
            InvokeRepeating(nameof(SpawnFlame), spawnDelay, spawnDelay);
            resetCoroutine = Unbound.Instance.StartCoroutine(ClearFrameDamageTracking());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Unbound.Instance.StopCoroutine(resetCoroutine);
            CancelInvoke(nameof(SpawnFlame));
        }

        private void SpawnFlame()
        {
            var flameArea = Instantiate(Assets.FlameArea, player.transform.position, Quaternion.identity);
            flameArea.GetComponent<HotPotatoFlame>().Init(player);
            Destroy(flameArea, lifetime);
        }

        private IEnumerator ClearFrameDamageTracking()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                HotPotatoFlame.effectedPlayers.Clear();
            }
        }
    }

    public class HotPotatoFlame : MonoBehaviour
    {
        public static readonly int RANGE = 1;
        internal static HashSet<Player> effectedPlayers = new HashSet<Player>();

        private Player owner;
        private float damage = 1;

        public void Init(Player owner)
        {
            this.owner = owner;
        }

        void FixedUpdate()
        {
            if (!owner.data.view.IsMine) return;

            foreach (var obj in Physics2D.OverlapCircleAll(transform.position, RANGE))
            {
                var player = obj.GetComponent<Player>();
                if (player && player != owner && !effectedPlayers.Contains(player))
                {
                    effectedPlayers.Add(player);

                    var finalDamage = damage + (0.25f * owner.data.weaponHandler.gun.damage);
                    var damageDir = Vector3.Normalize(player.transform.position - transform.position) * finalDamage;
                    player.data.healthHandler.CallTakeDamage(damageDir, player.transform.position, null, owner);
                }
            }
        }
    }
}
