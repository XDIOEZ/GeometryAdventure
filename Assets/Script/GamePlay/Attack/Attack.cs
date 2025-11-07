using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : NetworkBehaviour
{
    [Header("攻击通用设置")]
    [SerializeField] protected string tagName = "Player";
    [SerializeField] protected float damageInterval = 0.5f;

    protected EntityData playerData;
    private readonly Dictionary<EntityData, Coroutine> damageCoroutines = new();

    void Start()
    {
        playerData = GetComponent<EntityData>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isServer) return;
        if (collision.gameObject.CompareTag(tagName)) return;

        EntityData otherPlayer = collision.gameObject.GetComponent<EntityData>();
        if (otherPlayer == null || otherPlayer.isInvincible) return;

        ProcessCombat(otherPlayer);

        if (!damageCoroutines.ContainsKey(otherPlayer))
        {
            Coroutine c = StartCoroutine(DealContinuousDamage(otherPlayer));
            damageCoroutines.Add(otherPlayer, c);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!isServer) return;

        EntityData otherPlayer = collision.gameObject.GetComponent<EntityData>();
        if (otherPlayer != null && damageCoroutines.ContainsKey(otherPlayer))
        {
            StopCoroutine(damageCoroutines[otherPlayer]);
            damageCoroutines.Remove(otherPlayer);
        }
    }

    private IEnumerator DealContinuousDamage(EntityData otherPlayer)
    {
        while (otherPlayer != null)
        {
            yield return new WaitForSeconds(damageInterval);
            if (!IsStillCollidingWith(otherPlayer))
                break;

            ProcessCombat(otherPlayer);
        }

        if (damageCoroutines.ContainsKey(otherPlayer))
            damageCoroutines.Remove(otherPlayer);
    }

    private bool IsStillCollidingWith(EntityData otherPlayer)
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        Collider2D otherCollider = otherPlayer.GetComponent<Collider2D>();
        if (myCollider == null || otherCollider == null)
            return false;

        return myCollider.IsTouching(otherCollider);
    }

    [Command]
    private void ProcessCombat(EntityData otherPlayer)
    {
        if (otherPlayer.hp <= 0) return;
        otherPlayer.CmdTakeDamage(-playerData.attack);
    }
}
