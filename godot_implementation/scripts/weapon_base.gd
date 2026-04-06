extends Node3D
class_name WeaponBase

# ─── Propriétés exportées ──────────────────────────────────────────────────────
@export var weapon_name: String  = "Arme"
@export var damage: float        = 25.0
@export var fire_rate: float     = 0.12   # secondes entre deux tirs
@export var max_ammo: int        = 30
@export var reserve_ammo: int    = 90
@export var reload_time: float   = 2.0
@export var is_automatic: bool   = true

# ─── État interne ──────────────────────────────────────────────────────────────
var current_ammo: int
var can_shoot: bool    = true
var is_reloading: bool = false
var audio_player: AudioStreamPlayer3D

# ─── Références (définies dans les scènes enfants) ────────────────────────────
@onready var shoot_timer:  Timer     = $ShootTimer
@onready var reload_timer: Timer     = $ReloadTimer
@onready var ray_cast:     RayCast3D = $RayCast3D

# ─── Initialisation ────────────────────────────────────────────────────────────
func _ready() -> void:
	current_ammo = max_ammo

	shoot_timer.wait_time = fire_rate
	shoot_timer.one_shot  = true
	shoot_timer.timeout.connect(_on_shoot_timer_timeout)

	reload_timer.wait_time = reload_time
	reload_timer.one_shot  = true
	reload_timer.timeout.connect(_on_reload_timer_timeout)

	audio_player = AudioStreamPlayer3D.new()
	audio_player.max_distance = 50.0
	audio_player.stream = _create_fire_sound()
	add_child(audio_player)

	_build_mesh()

func _create_fire_sound() -> AudioStreamWAV:
	return null

# ─── Interface publique ────────────────────────────────────────────────────────
func try_shoot() -> void:
	if not can_shoot or is_reloading:
		return
	if current_ammo <= 0:
		reload()
		return
	_shoot()

func reload() -> void:
	if is_reloading or current_ammo == max_ammo or reserve_ammo <= 0:
		return
	is_reloading = true
	reload_timer.start()

# ─── Tir interne ───────────────────────────────────────────────────────────────
func _shoot() -> void:
	can_shoot = false
	current_ammo -= 1
	_perform_raycast()
	GameManager.ammo_changed.emit(current_ammo, reserve_ammo)
	_play_fire_sound()
	shoot_timer.start()

func _play_fire_sound() -> void:
	if audio_player.stream:
		audio_player.play()

func _perform_raycast() -> void:
	ray_cast.force_raycast_update()
	if ray_cast.is_colliding():
		var hit = ray_cast.get_collider()
		if hit and hit.has_method("take_damage"):
			hit.take_damage(damage)

# ─── Callbacks timers ──────────────────────────────────────────────────────────
func _on_shoot_timer_timeout() -> void:
	can_shoot = true

func _on_reload_timer_timeout() -> void:
	var needed    : int = max_ammo - current_ammo
	var available : int = mini(needed, reserve_ammo)
	current_ammo  += available
	reserve_ammo  -= available
	is_reloading   = false
	GameManager.ammo_changed.emit(current_ammo, reserve_ammo)

# ─── Visuel placeholder (override dans les sous-classes) ──────────────────────
func _build_mesh() -> void:
	pass
