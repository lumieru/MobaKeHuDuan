using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;

public class BulletLinearFly : MonoBehaviour
{
    public Vector3 OffsetPos;
    public SkillLayoutRunner runner;
    public MissileData missileData;

    private GameObject attacker;
    private GameObject target;
    private SkillData skillData;

    private Vector3 initPos;
    private float flyTime;
    private Vector3 dieTargetPos;
    private float passTime = 0;

    void Start()
    {
        attacker = runner.stateMachine.attacker;
        target = runner.stateMachine.target;
        skillData = runner.stateMachine.skillFullData.skillData;

        if (missileData.ReleaseParticle != null)
        {
            GameObject par = Instantiate(missileData.ReleaseParticle) as GameObject;
            NGUITools.AddMissingComponent<RemoveSelf>(par);

            var playerForward =
                Quaternion.Euler(new Vector3(0, 0 + attacker.transform.rotation.eulerAngles.y, 0));
            par.transform.parent = ObjectManager.objectManager.transform;
            par.transform.localPosition = attacker.transform.localPosition + playerForward * OffsetPos;
            par.transform.localRotation = playerForward;
        }

        //飞行粒子效果
        if (missileData.ActiveParticle != null)
        {
            GameObject par = Instantiate(missileData.ActiveParticle) as GameObject;
            par.transform.parent = transform;
            par.transform.localPosition = Vector3.zero;
            par.transform.localRotation = Quaternion.identity;
        }

        initPos = transform.position;
        var tarPos = initPos + transform.forward * missileData.Velocity * missileData.lifeTime;
        flyTime = missileData.lifeTime;
        dieTargetPos = tarPos;
        flyTime = Mathf.Max(flyTime, 0.5f);
    }

    private void FixedUpdate()
    {
        passTime += Time.fixedDeltaTime;
        var rate = Mathf.Clamp01(passTime / flyTime);
        var newPos = Vector3.Lerp(initPos, dieTargetPos, rate);
        newPos.y = initPos.y;
        transform.position = newPos;

        if (passTime >= flyTime)
        {
            HitSomething();
        }
    }


    private void HitSomething()
    {
        CreateHitParticle();
        GameObject.Destroy(gameObject);
    }

    private void CreateHitParticle()
    {
        if (missileData.HitParticle != null)
        {
            var g = ParticlePool.Instance.GetGameObject(missileData.HitParticle, ParticlePool.InitParticle);
            var removeSelf = NGUITools.AddMissingComponent<DumpMono>(g);
            removeSelf.StartCoroutine(DestoryBullet(g));
            g.transform.position = transform.position;
            g.transform.parent = ObjectManager.objectManager.transform;
        }
        MakeSound();
        CreateCameraShake();
    }

    private IEnumerator DestoryBullet(GameObject go)
    {
        yield return new WaitForSeconds(2);
        ParticlePool.Instance.ReturnGameObject(go, ParticlePool.ResetParticle);
    }
    private void MakeSound()
    {
        if (!string.IsNullOrEmpty(skillData.HitSound))
        {
            BackgroundSound.Instance.PlayEffect(skillData.HitSound, 0.3f);
        }
    }

    private void CreateCameraShake()
    {
        if (attacker != null && attacker.GetComponent<KBEngine.KBNetworkView>().IsMe)
        {
            if (missileData.shakeData != null)
            {
                var shakeObj = new GameObject("CameraShake");
                shakeObj.transform.parent = ObjectManager.objectManager.transform;
                var shake = shakeObj.AddComponent<CameraShake>();
                shake.shakeData = missileData.shakeData;
                shake.autoRemove = true;
            }
        }
    }
}
