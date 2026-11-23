using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TowerDefense.Data;

namespace TowerDefense.Utils
{
    public class SpriteManager : MonoBehaviour
    {
        public static SpriteManager Instance { get; private set; }

        private Dictionary<string, Sprite> _loadedSprites = new Dictionary<string, Sprite>();

        private void Awake()
        {
            Instance = this;
            LoadAllSprites();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void LoadAllSprites()
        {
            string spritesPath = Path.Combine(Application.dataPath, "Sprites");
            if (Directory.Exists(spritesPath))
            {
                string[] files = Directory.GetFiles(spritesPath, "*.png");
                foreach (string file in files)
                {
                    string spriteName = Path.GetFileNameWithoutExtension(file);
                    byte[] bytes = File.ReadAllBytes(file);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (tex.LoadImage(bytes))
                    {
                        tex.filterMode = FilterMode.Bilinear;
                        tex.wrapMode = TextureWrapMode.Clamp;
                        Sprite sprite = Sprite.Create(
                            tex,
                            new Rect(0, 0, tex.width, tex.height),
                            new Vector2(0.5f, 0.5f),
                            64f // 64 пикселя на единицу сетки
                        );
                        _loadedSprites[spriteName] = sprite;
                    }
                }
            }
        }

        public Sprite GetSprite(string name)
        {
            if (_loadedSprites.TryGetValue(name, out var sprite))
            {
                return sprite;
            }

            // Fallback процедурный спрайт, если файл не найден
            return GetOrCreateFallbackSprite(name);
        }

        private Sprite GetOrCreateFallbackSprite(string name)
        {
            if (_loadedSprites.TryGetValue("fallback_" + name, out var cached))
                return cached;

            Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color fill = Color.white;
            if (name.Contains("grunt")) fill = Color.red;
            else if (name.Contains("scout")) fill = Color.yellow;
            else if (name.Contains("golem")) fill = new Color(0.6f, 0.2f, 0.8f);
            else if (name.Contains("path")) fill = new Color(0.4f, 0.35f, 0.3f);
            else if (name.Contains("buildable")) fill = new Color(0.2f, 0.5f, 0.3f);
            else if (name.Contains("deadzone")) fill = new Color(0.15f, 0.15f, 0.18f);
            else if (name.Contains("frost")) fill = Color.cyan;
            else if (name.Contains("mortar")) fill = new Color(0.8f, 0.4f, 0.1f);
            else if (name.Contains("archer")) fill = new Color(0.2f, 0.6f, 0.9f);

            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++) colors[i] = fill;
            tex.SetPixels(colors);
            tex.Apply();

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
            _loadedSprites["fallback_" + name] = sprite;
            return sprite;
        }

        public Sprite GetTileSprite(ZoneType zone)
        {
            switch (zone)
            {
                case ZoneType.PathA: return GetSprite("tile_path");
                case ZoneType.BuildableB: return GetSprite("tile_buildable");
                case ZoneType.DeadZoneD: return GetSprite("tile_deadzone");
                case ZoneType.BaseC: return GetSprite("base_crystal");
                default: return GetSprite("tile_buildable");
            }
        }

        public Sprite GetTowerBaseSprite(TowerType type)
        {
            switch (type)
            {
                case TowerType.Archer: return GetSprite("tower_archer_base");
                case TowerType.Frost: return GetSprite("tower_frost_base");
                case TowerType.Mortar: return GetSprite("tower_mortar_base");
                default: return GetSprite("tower_archer_base");
            }
        }

        public Sprite GetTowerTurretSprite(TowerType type)
        {
            switch (type)
            {
                case TowerType.Archer: return GetSprite("tower_archer_turret");
                case TowerType.Frost: return GetSprite("tower_frost_crystal");
                case TowerType.Mortar: return GetSprite("tower_mortar_barrel");
                default: return GetSprite("tower_archer_turret");
            }
        }

        public Sprite GetEnemySprite(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Grunt: return GetSprite("enemy_grunt");
                case EnemyType.Scout: return GetSprite("enemy_scout");
                case EnemyType.Golem: return GetSprite("enemy_golem");
                default: return GetSprite("enemy_grunt");
            }
        }

        public Sprite GetBulletSprite(TowerType type)
        {
            switch (type)
            {
                case TowerType.Archer: return GetSprite("bullet_arrow");
                case TowerType.Frost: return GetSprite("bullet_frost");
                case TowerType.Mortar: return GetSprite("bullet_mortar");
                default: return GetSprite("bullet_arrow");
            }
        }
    }
}
