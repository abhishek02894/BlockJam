using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Tag.Block
{
    public static class Utility
    {
        public static string GetPlayerPrefsSaveString(this DateTime dateTimeToSave)
        {
            return dateTimeToSave.ToString(CultureInfo.InvariantCulture);
        }
        public static long GetUnixTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        public static float GetClipRunTimeFromAnimator(Animator targetAnimator, string animationName)
        {
            AnimationClip[] clips = targetAnimator.runtimeAnimatorController.animationClips;

            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].name == animationName)
                    return clips[i].length;
            }

            return 0f;
        }

        public static bool IsInternetOn()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
                return false;
            return true;
        }
        public static T GetRandomItemFromList<T>(this List<T> listOfT)
        {
            if (listOfT.Count > 0)
                return listOfT[UnityEngine.Random.Range(0, listOfT.Count)];
            return default(T);
        }
    }
}