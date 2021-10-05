﻿using System;
using UnboundLib;
using HarmonyLib;
using UnityEngine;
using ModdingUtils.Utils;
using System.Linq;

namespace DeckCustomization
{
    // patch to use custom rarities
    [Serializable]
    [HarmonyPatch(typeof(CardChoice), "GetRanomCard")]
    class CardChoicePatchGetRanomCard
    {
        private static bool Prefix(ref GameObject __result)
        {
			if (DeckCustomization.BetterMethod)
            {
				__result = GetRandomCard.Efficient(CardChoice.instance);
            }
			else
            {
				__result = GetRandomCard.Modified(CardChoice.instance);
            }

			return false; // always skip original

        }
    }

    internal static class GetRandomCard
    {
        internal static GameObject Modified(CardChoice cardChoice)
        {
			// a modified version of the base-game method in order to be sure that everything is as close to vanilla as possible
			GameObject result = null;
			float num = 0f;
			for (int i = 0; i < cardChoice.cards.Length; i++)
			{
				num += RarityUtils.GetRelativeRarity(cardChoice.cards[i]);
			}
			float num2 = UnityEngine.Random.Range(0f, num);
			for (int j = 0; j < cardChoice.cards.Length; j++)
			{
				num2 -= RarityUtils.GetRelativeRarity(cardChoice.cards[j]);

				if (num2 <= 0f)
				{
					result = cardChoice.cards[j].gameObject;
					break;
				}
			}
			return result;
		}
        internal static GameObject Efficient(CardChoice cardChoice)
        {
			// this is a more efficient version of the above method that gauruntees that cards drawn will be valid on the first try
			Player player = null;
			if ((PickerType)cardChoice.GetFieldValue("pickerType") == PickerType.Team)
			{
				player = PlayerManager.instance.GetPlayersInTeam(cardChoice.pickrID)[0];
			}
			else
			{
				player = PlayerManager.instance.players[cardChoice.pickrID];
			}
			if (player == null) { return Modified(cardChoice); }

			CardInfo[] validCards = cardChoice.cards.Where(c => ModdingUtils.Utils.Cards.instance.PlayerIsAllowedCard(player, c)).ToArray();

			GameObject result = null;
			float num = 0f;
			for (int i = 0; i < validCards.Length; i++)
			{
				num += RarityUtils.GetRelativeRarity(validCards[i]);
			}
			float num2 = UnityEngine.Random.Range(0f, num);
			for (int j = 0; j < validCards.Length; j++)
			{
				num2 -= RarityUtils.GetRelativeRarity(validCards[j]);

				if (num2 <= 0f)
				{
					result = validCards[j].gameObject;
					break;
				}
			}
			return result;

		}
    }


}
