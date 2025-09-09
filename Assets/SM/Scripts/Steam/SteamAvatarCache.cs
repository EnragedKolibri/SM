using System.Collections.Generic;
using UnityEngine;
using Steamworks;

namespace SM.Steam
{
    /// <summary>
    /// Small helper to fetch and cache Steam avatars (small size) as Texture2D.
    /// </summary>
    public static class SteamAvatarCache
    {
        private static readonly Dictionary<CSteamID, Texture2D> _cache = new();

        public static Texture2D GetSmallAvatar(CSteamID id)
        {
            if (_cache.TryGetValue(id, out var tex))
                return tex;

            int imageId = SteamFriends.GetSmallFriendAvatar(id);
            if (imageId == -1) return null;

            if (!SteamUtils.GetImageSize(imageId, out uint w, out uint h) || w == 0 || h == 0)
                return null;

            var raw = new byte[4 * w * h];
            if (!SteamUtils.GetImageRGBA(imageId, raw, (int)raw.Length)) return null;

            var t = new Texture2D((int)w, (int)h, TextureFormat.RGBA32, false);
            t.LoadRawTextureData(raw);
            t.Apply();
            _cache[id] = t;

            Debug.Log($"[SteamAvatarCache] Cached avatar for {id.m_SteamID} {w}x{h}");
            return t;
        }
    }
}
