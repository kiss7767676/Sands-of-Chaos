using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class Bullet : MonoBehaviour
{
    private int bulletDamage;
    private float impactForce;
    private BoxCollider cd;
    private Rigidbody rb;
    private MeshRenderer meshRenderer;
    private TrailRenderer trailRenderer;
    [SerializeField] private GameObject bulletImpactFX;


    private Vector3 startPosition;
    private float flyDistance;
    private bool bulletsDisabled;

    private LayerMask allyLayerMask;

    protected virtual void Awake()
    {
        cd = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    public void BulletSetup(LayerMask allyLayerMask, int bulletDamage, float flyDistance = 100, float impactForce = 100)
    {
        this.allyLayerMask = allyLayerMask;
        this.impactForce = impactForce;
        this.bulletDamage = bulletDamage;


        bulletsDisabled = false;
        cd.enabled = true;
        meshRenderer.enabled = true;

        trailRenderer.Clear();
        trailRenderer.time = 0.25f;
        startPosition = transform.position;
        this.flyDistance = flyDistance + .5f;
    }

    protected virtual void Update()
    {
        FadeTrailIfNeeded();
        DisableBulletsIfNeeded();
        ReturnToPoolIfNeeded();
    }

    protected void ReturnToPoolIfNeeded()
    {
        if (trailRenderer.time < 0)
        {
            ReturnBulletToPool();
        }
    }

    protected void DisableBulletsIfNeeded()
    {
        if (Vector3.Distance(startPosition, transform.position) > flyDistance && !bulletsDisabled)
        {
            cd.enabled = false;
            meshRenderer.enabled = false;
            bulletsDisabled = true;
        }
    }

    protected void FadeTrailIfNeeded()
    {
        if (Vector3.Distance(startPosition, transform.position) > flyDistance - 1.5f)
        {
            trailRenderer.time -= 2 * Time.deltaTime;
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (FriendlyFire() == false)
        {
            // Use a bitwise AND to check if the collsion layer is in the allyLayerMask
            if ((allyLayerMask.value & (1 << collision.gameObject.layer)) > 0)
            {
                ReturnBulletToPool(10);
                return;
            }
        }

        CreateImpactFX();
        ReturnBulletToPool();

        IDamagable damagable = collision.gameObject.GetComponent<IDamagable>();
        damagable?.TakeDamage(bulletDamage);


        ApplyBulletImpactToEnemy(collision);

    }

    private void ApplyBulletImpactToEnemy(Collision collision)
    {
        Enemy enemy = collision.gameObject.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            Vector3 force = rb.linearVelocity.normalized * impactForce;
            Rigidbody hitRigidbody = collision.collider.attachedRigidbody;
            enemy.BulletImpact(force, collision.contacts[0].point, hitRigidbody);
        }
    }

    protected void ReturnBulletToPool(float delay = 0) => ObjectPool.Instance.ReturnObject(gameObject, delay);
    protected void CreateImpactFX()
    {
        GameObject newImpactFX = Instantiate(bulletImpactFX);
        newImpactFX.transform.position = transform.position;

        Destroy(newImpactFX, 1);

        // GameObject newImpactFX = ObjectPool.Instance.GetObject(bulletImpactFX, transform);
        // ObjectPool.Instance.ReturnObject(newImpactFX, 1);

    }

    private bool FriendlyFire() => GameManager.instance.friendlyFire;
}
