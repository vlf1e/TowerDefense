using UnityEngine;
using TowerDefense.Data;
using TowerDefense.Enemies;
using TowerDefense.Utils;

namespace TowerDefense.Towers
{
    public class Projectile : MonoBehaviour
    {
        private TowerType _towerType;
        private Enemy _target;
        private Vector3 _targetLastPos;
        private float _damage;
        private float _speed;
        private AttackType _attackType;
        private float _slowFactor;
        private float _slowDuration;
        private float _splashRadius;

        private SpriteRenderer _renderer;

        public void Initialize(
            TowerType towerType,
            Enemy target,
            float damage,
            float speed,
            AttackType attackType,
            float slowFactor,
            float slowDuration,
            float splashRadius
        )
        {
            _towerType = towerType;
            _target = target;
            _damage = damage;
            _speed = speed;
            _attackType = attackType;
            _slowFactor = slowFactor;
            _slowDuration = slowDuration;
            _splashRadius = splashRadius;

            if (_target != null)
            {
                _targetLastPos = _target.transform.position;
            }

            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer == null)
            {
                _renderer = gameObject.AddComponent<SpriteRenderer>();
            }

            _renderer.sprite = SpriteManager.Instance.GetBulletSprite(towerType);
            _renderer.sortingOrder = 8;
        }

        private void Update()
        {
            if (_target != null && !_target.IsDead)
            {
                _targetLastPos = _target.transform.position;
            }

            Vector3 dir = (_targetLastPos - transform.position);
            float step = _speed * Time.deltaTime;

            // Поворот снаряда по направлению движения
            if (dir.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            if (dir.magnitude <= step)
            {
                HitTarget();
            }
            else
            {
                transform.position += dir.normalized * step;
            }
        }

        private void HitTarget()
        {
            if (_attackType == AttackType.AreaOfEffect)
            {
                // Урон по площади (Splash)
                if (WaveManager.Instance != null)
                {
                    var enemies = WaveManager.Instance.GetEnemiesInSplashRadius(_targetLastPos, _splashRadius);
                    foreach (var enemy in enemies)
                    {
                        if (enemy != null && !enemy.IsDead)
                        {
                            enemy.TakeDamage(_damage);
                        }
                    }
                }

                // Эффект взрыва мортиры
                CreateExplosionEffect(_targetLastPos, _splashRadius);
            }
            else
            {
                // Одиночный урон или замедление
                if (_target != null && !_target.IsDead)
                {
                    _target.TakeDamage(_damage);

                    if (_attackType == AttackType.Slow)
                    {
                        _target.ApplySlow(_slowFactor, _slowDuration);
                    }
                }
            }

            Destroy(gameObject);
        }

        private void CreateExplosionEffect(Vector3 pos, float radius)
        {
            GameObject fx = new GameObject("SplashFX");
            fx.transform.position = pos;
            fx.transform.localScale = Vector3.one * (radius * 1.5f);

            SpriteRenderer fxRend = fx.AddComponent<SpriteRenderer>();
            fxRend.sprite = SpriteManager.Instance.GetSprite("circle_splash");
            fxRend.sortingOrder = 9;

            Destroy(fx, 0.25f);
        }
    }
}
