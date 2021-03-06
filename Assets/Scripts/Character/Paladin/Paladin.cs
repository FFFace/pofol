﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Paladin : Character
{
    private PaladinNormalAttack attack;
    //private PaladinSlashAttack slash;
    //private PaladinChainAttack chain;
    //private PaladinBlock block;

    [SerializeField]
    private ParticleSystem buff;

    [SerializeField]
    private Sprite SlashImage;
    [SerializeField]
    private Sprite ChainImage;
    [SerializeField]
    private Sprite BlockImage;

    [SerializeField]
    private PaladinWeapon weapon;

    private bool isChain;
    public bool isBlock;
    private bool isJustBlock;
    private IEnumerator IEnumbuff;

    protected override void InitData()
    {
        base.InitData();

        state.hp = 50;
        state.attackDamage = 2;
        state.attackSpeed = 1;
        state.moveSpeed = 5;
        state.jumpPower = 5;

        currentMoveSpeed = state.moveSpeed;
        currentAttackDamage = state.attackDamage;
        currentAttackSpeed = state.attackSpeed;
        currentJumpPower = state.jumpPower;
        currentCharacterHP = state.hp;
        currentDefense = state.defense;
        currentKnockBackPower = 0;

        attack = new PaladinNormalAttack(weapon);
        //slash = new PaladinSlashAttack(weapon, 10.0f, 1.5f, SlashImage);
        //chain = new PaladinChainAttack(weapon, 3.0f, 1.7f, ChainImage);
        //block = new PaladinBlock(this, 1.5f, 10.0f, buff, BlockImage);

        normalAttack = attack;
        //mainSkill = CharacterInfo.instance.GetSkills(0); //chain;
        //subSkill = CharacterInfo.instance.GetSkills(1); //slash;
        //subAttack = CharacterInfo.instance.GetSkills(2);  //block;

        isChain = false;

        UIManager.instance.SetSkillImage(mainSkill.GetImage(), subSkill.GetImage(), subAttack.GetImage());
        UIManager.instance.SetSkillCoolTime(mainSkill.GetCoolTime(), subSkill.GetCoolTime(), subAttack.GetCoolTime());
        UIManager.instance.SetPlayerMaxHPBar(state.hp);

        if (mainSkill is PaladinBlock) IEnumbuff = (mainSkill as PaladinBlock).JustBlockBuff();
        else if (subSkill is PaladinBlock) IEnumbuff = (subSkill as PaladinBlock).JustBlockBuff();
        else if (subAttack is PaladinBlock) IEnumbuff = (subAttack as PaladinBlock).JustBlockBuff();
    }

    public override void Hit(float damage, Vector3 direction)
    {
        float dot = Vector3.Dot(transform.forward, direction);

        if (isBlock && dot < 0)
        {
            if (isJustBlock)
            {
                StopCoroutine(IEnumbuff);
                StartCoroutine(IEnumbuff);
                damage = 0;
            }

            else
            {
                damage = damage * 0.5f;
            }
        }

        damage -= damage * currentDefense;
        damage = damage < 0 ? 0 : damage;
        currentCharacterHP -= damage;

        if (currentCharacterHP <= 0)
        {
            PlayerControll.instance.SetDeathState(true);
            ResetAnimation();
            SetAnimationBool("Dead", true);
            return;
        }

        SetAnimationTrigger("Hit");
        SetAnimationLayerWeight(1, 1);
        UIManager.instance.PlayerHit(currentCharacterHP);
    }

    public override void SubAttackPressDown()
    {
        if (!isAction)
        {
            isAttack = true;
            isAction = true;
            ResetAnimation();
            subAttack.SkillKeyDown();
        }
    }

    public override void SubAttackPressUp()
    {
        if (!isAction)
        {
            isAttack = true;
            isAction = true;
            ResetAnimation();
            subAttack.SkillKeyUp();
        }
    }

    public override void MainSkill()
    {
        if (!isAction)
        {
            isAttack = true;
            isAction = true;
            ResetAnimation();
            mainSkill.SkillKeyDown();
        }   
    }

    public override void SubSkill()
    {
        if (!isAction)
        {
            isAttack = true;
            isAction = true;
            ResetAnimation();
            subSkill.SkillKeyDown();
        }
    }

    public void AttackColliderOn()
    {
        weapon.Attack();
    }

    public void AttackColliderOff()
    {
        weapon.AttackEnd();
    }

    public override void AttackEnd()
    {
        base.AttackEnd();
        SetAnimationBool("Walk", false);
    }

    //public void OnChainAttack()
    //{
    //    isChain = false;
    //}

    public void OnChainAttack()
    {
        isChain = true;
    }

    public void ChainCombo()
    {
        //chain.isActive = false;
        isChain = false;
    }

    public void ChainComboEnd()
    {
        //chain.isActive = true;
        //isChain = true;
    }

    public bool GetChainState()
    {
        return isChain;
    }

    public void ChainCoolTime()
    {
        if (mainSkill is PaladinChainAttack) StartCoroutine((mainSkill as PaladinChainAttack).SkillCoolTime());
        else if (subSkill is PaladinChainAttack) StartCoroutine((subSkill as PaladinChainAttack).SkillCoolTime());
        else if (subAttack is PaladinChainAttack) StartCoroutine((subAttack as PaladinChainAttack).SkillCoolTime());

        UIManager.instance.SetMainSkillCoolTime(mainSkill.isActive);
        UIManager.instance.SetSubSkillCoolTime(subSkill.isActive);
        UIManager.instance.SetSubAttackCoolTime(subAttack.isActive);
    }

    public void JuskBlock()
    {
        isJustBlock = true;
    }

    public void JustBlockEnd()
    {
        isJustBlock = false;
    }

    public void BlockActive(bool _active)
    {
        isBlock = _active;
    }

    public void SetEnumStay(Animator _anim, int _layerIndex, float _time)
    {
        attack.SetEnumStay(_anim, _layerIndex, _time);
    }
}

public class PaladinNormalAttack : IAttackAction
{
    private Character character;
    public PaladinWeapon weapon { get; private set; }
    private IEnumerator enumStay;

    public PaladinNormalAttack(PaladinWeapon _weapon) { character = Character.instance; weapon = _weapon; }
    public PaladinNormalAttack() { }
    public void Attack()
    {
        weapon.SetDamage(character.GetCharacterCurrentDamage());
        weapon.KnockBack(false);
        character.SetAnimationTrigger("Attack");
        character.SetAnimationBool("Walk", true);
        character.StopCoroutine(enumStay);
        character.SetAnimationLayerWeight(1, 1);
    }

    public void SetEnumStay(Animator _anim, int _layerIndex, float _time)
    {
        enumStay = IEnumStay(_anim, _layerIndex, _time);
        character.StartCoroutine(enumStay);
    }

    private IEnumerator IEnumStay(Animator _anim, int _layerIndex, float _time)
    {
        yield return new WaitForSeconds(_time);
        _anim.SetLayerWeight(_layerIndex, 0);
    }

    public void SetWeapon(PaladinWeapon _weapon)
    {
        weapon = _weapon;
    }
}

public class PaladinSlashAttack : ISkill
{
    private Character character;
    private PaladinWeapon weapon;
    private float coolTime;
    private float damageMagnification;
    private Sprite image;
    public bool isActive { get; set; }

    public PaladinSlashAttack(PaladinWeapon _weapon, float _coolTime, float _damageManification, Sprite _sprite)
    {
        character = Character.instance;
        weapon = _weapon;
        coolTime = _coolTime;
        damageMagnification = _damageManification;
        image = _sprite;
    }

    public PaladinSlashAttack(float _coolTime, float _damageMagnification, Sprite _sprite)
    {
        character = Character.instance;
        coolTime = _coolTime;
        damageMagnification = _damageMagnification;
        image = _sprite;
    }

    public void SkillKeyDown()
    {
        if (!isActive)
        {
            character.ResetAnimation();
            character.ActionActive();
            weapon.SetDamage(character.GetCharacterCurrentDamage() * damageMagnification);
            weapon.KnockBack(true, 10);
            character.SetAnimationTrigger("Slash");
            character.StartCoroutine(SkillCoolTime());
        }
    }

    public void SkillKeyUp() { }

    public IEnumerator SkillCoolTime()
    {
        isActive = true;
        yield return new WaitForSeconds(coolTime);
        isActive = false;
    }

    public float GetDamage()
    {
        return character.GetCharacterCurrentDamage() * damageMagnification;
    }

    public float GetDamageMagnification()
    {
        return damageMagnification;
    }

    public Sprite GetImage()
    {
        return image;
    }

    public float GetCoolTime() { return coolTime; }

    public string GetExplain()
    {
        return "주변에 " + (damageMagnification * 100).ToString() + "% 만큼의 데미지를 주고 넉백 시킵니다.";
    }

    public void SetWeapon(PaladinWeapon _weapon)
    {
        weapon = _weapon;
    }

    public bool GetToggleState()
    {
        return false;
    }
}

public class PaladinChainAttack : ISkill
{
    private Paladin character;
    public PaladinWeapon weapon { get; private set; }
    private float coolTime;
    private float damageMagnification;
    private Sprite image;
    public bool isActive { get; set; }

    public PaladinChainAttack(PaladinWeapon _weapon, float _coolTime, float _damageManification, Sprite _sprite)
    {
        character = Character.instance as Paladin;
        weapon = _weapon;
        coolTime = _coolTime;
        damageMagnification = _damageManification;
        image = _sprite;
    }

    public PaladinChainAttack(float _coolTime, float _damageManification, Sprite _sprite)
    {
        character = Character.instance as Paladin;
        coolTime = _coolTime;
        damageMagnification = _damageManification;
        image = _sprite;
    }

    public void SkillKeyDown()
    {
        if (!isActive && character.GetChainState())
        {
            character.ResetAnimation();
            character.ActionActive();
            weapon.SetDamage(character.GetCharacterCurrentDamage() * damageMagnification);
            weapon.KnockBack(true, 5);
            character.SetAnimationTrigger("Chain");
        }
    }

    public void SkillKeyUp() { }

    public IEnumerator SkillCoolTime()
    {
        isActive = true;
        character.ChainCombo();
        yield return new WaitForSeconds(coolTime);

        //character.OffChainAttack();
        //int num = chainNum;

        //if (num != 3)
        //{
        //    yield return new WaitForSeconds(1.5f);
        //    Debug.Log(num.ToString() + ", " + chainNum.ToString());
        //    if (chainNum != num)
        //        yield return null;
        //    else
        //    {
        //        character.OffChainAttack();
        //        isActive = true;
        //        yield return new WaitForSeconds(coolTime);
        //        Debug.Log("B");
        //        isActive = false;
        //        character.OnChainAttack();
        //        chainNum = 0;
        //    }
        //}
        //else
        //{
        //    character.OffChainAttack();
        //    isActive = true;
        //    yield return new WaitForSeconds(coolTime);
        //    isActive = false;
        //    character.OnChainAttack();
        //    chainNum = 0;
        //}
    }

    public float GetDamage()
    {
        return character.GetCharacterCurrentDamage() * damageMagnification;
    }

    public float GetDamageMagnification()
    {
        return damageMagnification;
    }

    public Sprite GetImage()
    {
        return image;
    }

    public float GetCoolTime() { return coolTime; }

    public string GetExplain()
    {
        return "주변에 " + (damageMagnification * 100).ToString() + "% 만큼의 데미지를 주고 넉백 시킵니다. 총 3번 사용이 가능합니다.";
    }

    public void SetWeapon(PaladinWeapon _weapon)
    {
        weapon = _weapon;
    }

    public bool GetToggleState()
    {
        return false;
    }
}

public class PaladinBlock : ISkill
{
    private Paladin character;
    private float damageMagnification;
    private float buffTime;
    private ParticleSystem particle;
    private Sprite image;
    public bool isActive { get; set; }

    public PaladinBlock(Paladin _character, float _damageMagnification, float _buffTime, ParticleSystem _particle, Sprite _sprite)
    {
        character = _character;
        damageMagnification = _damageMagnification;
        buffTime = _buffTime;
        particle = _particle;
        image = _sprite;
        isActive = false;
    }

    public PaladinBlock(float _damageMagnification, float _buffTime, Sprite _sprite)
    {
        character = Character.instance as Paladin;
        damageMagnification = _damageMagnification;
        buffTime = _buffTime;
        image = _sprite;
    }

    public void SkillKeyDown()
    {
        character.ResetAnimation();
        character.ActionActive();
        character.SetAnimationBool("Block", true);
        character.BlockActive(true);
    }

    public void SkillKeyUp() 
    {
        character.SetAnimationBool("Block", false);
        character.BlockActive(false);
    }
    public IEnumerator JustBlockBuff()
    {
        float damage = character.GetCharacterState().attackDamage;

        character.SetCharacterCurrentDamage(damage * damageMagnification);
        Renderer[] renderer = character.GetComponentsInChildren<Renderer>();
        for(int i=0; i<renderer.Length; i++)
            renderer[i].material.SetFloat("_RedColor", 1);
        particle.gameObject.SetActive(true);
        yield return new WaitForSeconds(buffTime);
        character.SetCharacterCurrentDamage(damage);
        for (int i = 0; i < renderer.Length; i++)
            renderer[i].material.SetFloat("_RedColor", 0);
        particle.gameObject.SetActive(false);
    }

    public IEnumerator SkillCoolTime()
    {
        yield return null;
    }

    public float GetDamage()
    {
        return 0;
    }

    public float GetDamageMagnification()
    {
        return damageMagnification;
    }

    public Sprite GetImage()
    {
        return image;
    }

    public float GetCoolTime() { return 0; }

    public void SetParticle(ParticleSystem _particle)
    {
        particle = _particle;
    }

    public string GetExplain()
    {
        return "[기본 보조스킬]\n\n정면에서 들어오는 공격을 방어합니다. 정확한 타이밍에 방어를 성공하면 공격력이 " +
            (damageMagnification * 100).ToString() + "% 만큼 증가하는 버프가 " + buffTime.ToString() + "초 만큼 지속됩니다.";
    }

    public bool GetToggleState()
    {
        return false;
    }
}