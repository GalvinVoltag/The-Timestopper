using System;
using System.Collections.Generic;
using The_Timestopper.Internal;
using ULTRAKILL.Portal;
using UnityEngine;

namespace The_Timestopper.Physics
{
    public class RigidbodyStopper : MonoBehaviour, IFixedUpdateReceiver    // Added to all Rigidbodies when time stops
    {
        private static List<RigidbodyStopper> instances = new List<RigidbodyStopper>();
        public bool isRegistered { get; set; }

        public float localTimeScale = 1.0f; // local timescale, so that coins and stuff freeze slowly
        bool byDio;
        bool isRocket;
        bool isNail;
        Chainsaw chainsaw;
        bool isGib;
        bool wasProvidenceParryable;
        private FixedUpdateCaller fuc;
        private CustomGravity customGravity;
        public Enemy enemy;
        public EnemyIdentifier EID;
        private Grenade grenade;
        private GameObject freezeEffect;
        private GameObject landmineTrigger;
        public Vector3 unscaledVelocity = new Vector3(0, 0, 0);
        public Vector3 unscaledAngularVelocity = new Vector3(0, 0, 0);
        public Rigidbody R;
        private SimplePortalTraveler portalTraveller; // SimplePortalTraveller

        private bool registered = false;

        public static void FreezeAll()
        {
            foreach (var RS in instances)
            {
                try
                {
                    RS.Freeze();
                }
                catch (Exception e)
                {
                    Timestopper.mls.LogError( $"error while freezing body {RS?.gameObject.name} " + e);
                }
            }
        }
        public static void UnfreezeAll()
        {
            foreach (var RS in instances) 
            {
                try
                {
                    RS.UnFreeze();
                }
                catch (Exception e)
                {
                    Timestopper.mls.LogError( $"error while unfreezing body {RS?.gameObject.name} " + e);
                }
            }
        }

        private void Register()
        {
            if (registered) return;
            registered = true;
            instances.Add(this);
            if (Timestopper.TimeStop)
                Freeze();
            FixedUpdateCaller.RegisterFixedUpdate(this);
        }

        private void Unregister()
        {
            if (!registered) return;
            registered = false;
            instances.Remove(this);
            FixedUpdateCaller.UnregisterFixedUpdate(this);
        }

        public void Freeze()
        {
            landmineTrigger?.SetActive(false);
            if (!R) {
                enabled = false;
                Destroy(this);
                return;
            }

            if (R.IsSleeping()) return;
            byDio = Timestopper.realTimeScale <= 0.5f;
            if (enemy && EID)
            {
                if (EID.enemyType != EnemyType.Turret) EID.ignorePlayer = true;
                if (EID.enemyType == EnemyType.Providence)
                {
                    wasProvidenceParryable = enemy.parryable;
                    enemy.parryable = true;
                }
            }
            if (chainsaw)
                R.isKinematic = false;
            if (R.isKinematic) return;
            unscaledVelocity = R.velocity;
            unscaledAngularVelocity = R.angularVelocity;
            previousLocalTimeScale = localTimeScale;
        }

        private void UnFreeze()
        {
            landmineTrigger?.SetActive(true);
            if (!R) {
                this.enabled = false;
                Destroy(this);
                return;
            }
            if (enemy && EID)
            {
                EID.ignorePlayer = false;
                if (EID.enemyType == EnemyType.Providence)
                    enemy.parryable = wasProvidenceParryable;
            }
            if (chainsaw)
                R.isKinematic = false;

            if (R.isKinematic) return;
            localTimeScale = 1.0f;
            previousLocalTimeScale = 1;
            R.velocity = unscaledVelocity;
            R.angularVelocity = unscaledAngularVelocity;

        }

        private void OnPortalTravel(in PortalTravelDetails details)
        {
            unscaledVelocity = details.enterToExit * unscaledVelocity;
        }

        private void Awake()
        {
            R = gameObject.GetComponent<Rigidbody>();
            customGravity = gameObject.GetComponent<CustomGravity>();

            portalTraveller = GetComponent<SimplePortalTraveler>();
            if (portalTraveller)
                portalTraveller.onTravel += OnPortalTravel;
            
            if (!R) {
                this.enabled = false;
                Destroy(this);
                return;
            }
            isGib = GetComponent<GoreSplatter>();
            grenade = GetComponent<Grenade>();
            chainsaw = GetComponent<Chainsaw>();
            enemy = GetComponent<Enemy>();
            EID = enemy?.EID;
            Nail nail = GetComponent<Nail>();
            if (nail)
            {
                if (nail.sawblade || nail.chainsaw)
                {
                    // if (GetComponent<FixedUpdateCaller>() == null)
                    gameObject.GetOrAddComponent<FixedUpdateCaller>();
                }
                isNail = true;
            }
            if (GetComponent<Landmine>() != null) landmineTrigger = transform.Find("Trigger").gameObject;
            if (chainsaw)
            {
                if (chainsaw.attachedTransform == Timestopper.Dummy?.transform)
                    chainsaw.attachedTransform = Timestopper.Player.transform.Find("Main Camera/New Game Object");
                gameObject.GetOrAddComponent<FixedUpdateCaller>().targets = new Component[] { chainsaw, GetComponent<SimplePortalTraveler>() } ;
            }
            // if (GetComponent<FixedUpdateCaller>() == null)
            if (grenade)
            {
                isRocket = grenade.rocket;
                fuc = grenade.GetOrAddComponent<FixedUpdateCaller>();
                fuc.targets = new Component[] { grenade };
                fuc.enabled = false;
            }
            if (isRocket) freezeEffect = transform.Find("FreezeEffect")?.gameObject;
            if (!R.isKinematic || enemy || landmineTrigger)
                Register();
        }

        public void OnDestroy()
        {
            instances.Remove(this);
            FixedUpdateCaller.UnregisterFixedUpdate(this);
        }

        public void Update()
        {
            if (!R) {
                enabled = false;
                Destroy(this);
                return;
            } // destroy if without rigidbody
            
            if (!R.isKinematic && !registered) Register();
            if (!enemy && !landmineTrigger && R.isKinematic && registered) Unregister();
            
            if (EID && EID.hooked && LightEnemies.Contains(EID.enemyType))
            {
                R.isKinematic = false;
                localTimeScale = 1;
                unscaledVelocity = R.velocity;
                return;
            }

            if (chainsaw)
            {
                if (R.isKinematic) R.isKinematic = false;
                if (chainsaw.attachedTransform == Timestopper.Dummy?.transform)
                    chainsaw.attachedTransform = Timestopper.Player.transform.Find("Main Camera/New Game Object");
            }
            
            if (!Timestopper.TimeStop || !isRocket) return;
            R.isKinematic = !grenade.frozen;
            grenade.rideable = true;
            fuc.FakeFixedUpdate();
            R.isKinematic = false;
            if (!Timestopper.TimeStop) previousLocalTimeScale = 1;
        }

        private static readonly HashSet<EnemyType> LightEnemies = new HashSet<EnemyType>(){
            EnemyType.Drone,
            EnemyType.Filth,
            EnemyType.Schism,
            EnemyType.Soldier,
            EnemyType.Stray,
            EnemyType.Streetcleaner
        };
        private float previousLocalTimeScale = 1;
        public void FakeFixedUpdate()
        {
            if (!R) {
                Destroy(this);
                return;
            }

            if (R.IsSleeping()) return;
            if ((isGib && localTimeScale == 0)) {
                R.Sleep();
                return;
            }

            // if (R.isKinematic)
            // {
            //     unscaledVelocity = Vector3.zero;
            //     unscaledAngularVelocity = Vector3.zero;
            //     return;
            // }

            if (!isRocket)
            {
                Vector3 velocityChange = (R.velocity - unscaledVelocity * previousLocalTimeScale);
                if (R.useGravity)
                {
                    R.AddForce(-UnityEngine.Physics.gravity * (1 - localTimeScale), ForceMode.Acceleration);
                }
                unscaledVelocity += velocityChange;
                
                unscaledAngularVelocity += R.angularVelocity - unscaledAngularVelocity * previousLocalTimeScale;
                
                if (customGravity && customGravity.useGravity) unscaledVelocity +=  customGravity.gravity * (Time.fixedDeltaTime * localTimeScale);
                
                R.velocity = unscaledVelocity * (localTimeScale);
                R.angularVelocity = unscaledAngularVelocity * (localTimeScale);
                
                previousLocalTimeScale = localTimeScale;
                
                
                if (isNail)
                    localTimeScale -= Time.fixedDeltaTime / Timestopper.stopSpeed.value * 64;
                else if (byDio)
                    localTimeScale -= Time.fixedDeltaTime / Timestopper.affectSpeed.value;
                else
                    localTimeScale -= Time.fixedDeltaTime / Timestopper.stopSpeed.value;
                if (localTimeScale < 0)
                    localTimeScale = 0.0f;
            } 
            else if ((MonoSingleton<WeaponCharges>.Instance && MonoSingleton<WeaponCharges>.Instance.rocketFrozen))
            {
                MonoSingleton<WeaponCharges>.Instance.rocketFrozen = false;
                MonoSingleton<WeaponCharges>.Instance.infiniteRocketRide = true;
                grenade.rideable = true;
                fuc.FakeFixedUpdate();
                MonoSingleton<WeaponCharges>.Instance.infiniteRocketRide = false;
                freezeEffect.SetActive(true);
                MonoSingleton<WeaponCharges>.Instance.rocketFrozen = true;
                if (localTimeScale < 1)
                    localTimeScale += Timestopper.playerDeltaTime / Timestopper.stopSpeed.value;
                if (localTimeScale > 1)
                    localTimeScale = 1.0f;
            }
        }
    }
}