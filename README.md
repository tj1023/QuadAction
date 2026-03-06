# 🟪 Quad Action
> **Unity 6.2 기반 3D 쿼터뷰 액션 게임 프로젝트**

## 📖 프로젝트 개요
* **개발 엔진**: Unity 6.2
* **사용 언어**: C#
* **장르**: 3D Quarter-view Action
* **개발 인원**: 1인 개발
* **특징**: 다양한 무기(근접, 원거리, 투척)를 활용한 전략적 전투와 끊임없이 몰려오는 적들을 상대하는 웨이브 기반 액션

## 🎮 주요 게임 특징 (Features)
* **다이나믹한 전투 시스템**: 
  * 플레이어는 3개의 무기 슬롯(주무기, 보조무기, 유틸리티)을 활용해 전투를 수행합니다.
  * **원거리 무기 (Handgun, SMG 등)**: 총알 속도와 발사 속도가 다르게 설정되며, 주무기일 경우 탄창과 예비 탄약(Ammo) 시스템으로 관리됩니다.
  * **근접 무기 (Hammer, Sword 등)**: 넉백 수치를 가지며, 피격 시 타격감을 극대화합니다. 애니메이션에 맞춘 정교한 히트박스(MeleeHitbox)로 판정됩니다.
  * **투척 무기 (Grenade)**: 물리(Rigidbody) 기반의 포물선 궤적 계산을 통해 투척되며, 폭발 반경(ExplosionRadius) 내 적에게 광역 피해를 주고 래그돌 물리 효과를 일으킵니다.
* **쿼터뷰 조작계**: 
  * `Input System`의 InputActionReference를 사용하여 마우스의 3D 바닥 좌표(Floor Layer)를 계산하는 에이밍 로직을 구현했습니다.
  * 이동 시 카메라의 Forward/Right 방향을 수평면(XZ 평면)에 투영 및 정규화하여, 카메라 시점에 상대적인 직관적인 키보드 이동(`GetCameraRelativeDirection`)을 지원합니다.
* **다양한 적 AI**: 
  * FSM(유한 상태 기계) 기반으로 몬스터(Idle → Chase → Dash → Attack) 및 보스 패턴이 구현되어 있습니다.
  * NavMeshAgent를 통해 플레이어를 추적하며 특정 거리 내 진입 시 플레이어를 향해 짧게 돌진(Dash)하거나 지정된 거리에서 근접/원거리 공격을 수행합니다. 폭발(수류탄 등)에 피격될 경우 일시적으로 NavMeshAgent가 비활성화되고 Rigidbody 물리(래그돌) 기반으로 날아갑니다.
* **아이템 시스템**: 
  * 적 처치 시 지정된 드랍 확률에 따라 필드에 소모품(ConsumableItem)이 스폰됩니다.
  * 플레이어가 접근(Trigger) 시 자동으로 획득되며, 체력(Heart), 탄약(Ammo), 재화(Coin) 3가지 타입의 보상과 플레이어 스탯이 연동됩니다. 상점을 통해 획득한 코인으로 무기 업그레이드가 가능합니다.

## 🛠 기술적 특징 (Technical Highlights)
* **Event-Driven Architecture**: `EventManager` 클래스를 활용한 Observer 패턴 적용. 플레이어 HP 변화, 무기 교체, 탄약 소비, 적 처치 등의 핵심 이벤트 처리가 다른 MonoBehaviour와 결합도 없이 독립적으로 동작하여 유지보수성을 극대화했습니다. UI 및 오디오 레이어가 완전히 분리되어 작동합니다.
* **Object Pooling**: `ObjectPool` 매니저를 통하여 빈번히 생성/파괴되는 총알(Bullet), 각종 시각 이펙트(PooledEffect), 적 몬스터의 메모리 할당(GC) 발생을 완전히 제거했습니다. 프리팹의 `InstanceID`를 고유 풀 키로 매핑하여 직관적이고 빠르게 관리됩니다.
* **Data-Driven Design**: 무기와 적 data를 `ScriptableObject`(WeaponData, EnemyData)로 분리해 기획 데이터와 핵심 로직 간 의존성을 낮췄습니다. 런타임에 불변하는 데이터 설정값만을 보관하며 런타임 상태 관리는 독립적인 객체 인스턴스가 담당합니다.
* **Animation Event Handling**: `AnimationEventHandler` 컴포넌트를 각각(Player/Enemy/Boss) 활용하여 공격의 판정 가능 프레임 한정, 재장전 및 회피 종료 순간 등 정교한 타이밍을 유니티 애니메이션 진행률과 완벽하게 동기화합니다.

## 📂 프로젝트 구조 (Project Structure)
* **`Core/`**: EventManager, ObjectPool, StageManager, SoundManager 등 게임 전반의 흐름 제어, 이벤트 시스템 및 메모리를 관리하는 폴더.
* **`Player/`**: PlayerController, PlayerWeaponManager, PlayerStats 등 플레이어의 이동, 공격 액션, 스테이터스 관리 핵심 로직 폴더.
* **`Enemy/`**: FSM 기반 인공지능 (EnemyController, BossController) 구동체와 히트박스 판정 스크립트를 포함하는 적 몬스터 전용 폴더.
* **`Item/`**: 필드 드랍되는 소모성 아이템 관리(ConsumableItem), 상점 교환 로직(Shop), 무기 데이터의 규격(WeaponData) 등을 포함하는 폴더.
* **`UI/`**: UIManager, 생명력 바, 총알 UI 등 EventManager에 구독하여 화면 출력값을 동기화하는 UI 스크립트 전용 관리 폴더.

## ⌨️ 조작 방법 (Controls)
* `W, A, S, D`: 이동
* `Mouse Left Click`: 기본 공격
* `Left Shift`: 회피 (Dodge)
* `E`: 상호작용
* `1, 2, 3` / `Mouse Wheel`: 무기 교체
* `R`: 재장전

## 🚀 시작하기 (Getting Started)
1. 이 레포지토리를 클론합니다: `git clone [레포지토리 URL]`
2. Unity Hub에서 `Unity 6.2` 버전으로 프로젝트를 엽니다.
3. `Assets/Scenes/SampleScene.unity`를 열고 Play 버튼을 누릅니다.
