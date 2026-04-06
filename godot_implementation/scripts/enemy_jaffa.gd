extends CharacterBody3D

# ─── États ─────────────────────────────────────────────────────────────────────
enum State { PATROL, CHASE, ATTACK, STUNNED, DEAD }

# ─── Paramètres ────────────────────────────────────────────────────────────────
@export var max_health: float      = 80.0
@export var walk_speed: float      = 2.5
@export var chase_speed: float     = 4.5
@export var detection_range: float = 18.0
@export var attack_range: float    = 7.0
@export var attack_damage: float   = 12.0
@export var attack_cooldown: float = 2.0

# ─── État interne ──────────────────────────────────────────────────────────────
var health: float
var state: State        = State.PATROL
var player: Node3D      = null
var can_attack: bool    = true
var stun_timer: float   = 0.0
var patrol_points: Array[Vector3] = []
var patrol_index: int   = 0

const GRAVITY := 9.8

# ─── Pivots d'animation ────────────────────────────────────────────────────────
var _pivot_left_leg:  Node3D
var _pivot_right_leg: Node3D
var _pivot_left_arm:  Node3D
var _pivot_right_arm: Node3D

var _anim_time:   float = 0.0   # accumulateur de temps pour la marche
var _attack_anim: float = 0.0   # [0..1] progression de l'animation d'attaque

# ─── Références ────────────────────────────────────────────────────────────────
@onready var attack_timer: Timer = $AttackTimer
var audio_player: AudioStreamPlayer3D

# ─── Initialisation ────────────────────────────────────────────────────────────
func _ready() -> void:
	health = max_health
	add_to_group("enemy")
	GameManager.register_enemy()

	attack_timer.wait_time = attack_cooldown
	attack_timer.one_shot  = true
	attack_timer.timeout.connect(func(): can_attack = true)

	var s := global_position
	patrol_points = [s, s + Vector3(4, 0, 0), s + Vector3(4, 0, 4), s + Vector3(0, 0, 4)]

	audio_player = AudioStreamPlayer3D.new()
	audio_player.max_distance = 50.0
	audio_player.stream = SoundGenerator.make_staff()
	add_child(audio_player)

	_build_mesh()

# ─── Boucle physique ───────────────────────────────────────────────────────────
func _physics_process(delta: float) -> void:
	if state == State.DEAD:
		return

	if not is_on_floor():
		velocity.y -= GRAVITY * delta

	if state == State.STUNNED:
		stun_timer -= delta
		velocity.x = move_toward(velocity.x, 0, 10.0)
		velocity.z = move_toward(velocity.z, 0, 10.0)
		if stun_timer <= 0.0:
			state = State.CHASE
		move_and_slide()
		return

	if state == State.PATROL:
		_scan_for_player()

	match state:
		State.PATROL: _do_patrol()
		State.CHASE:  _do_chase()
		State.ATTACK: _do_attack()

	move_and_slide()

func _process(delta: float) -> void:
	if state == State.DEAD:
		return
	_animate(delta)

# ─── Animation procédurale ─────────────────────────────────────────────────────
func _animate(delta: float) -> void:
	var is_moving := state == State.PATROL or state == State.CHASE

	# Accumulation du temps uniquement en mouvement
	if is_moving:
		_anim_time += delta * 5.0   # fréquence de pas

	# Décroissance de l'animation d'attaque
	if _attack_anim > 0.0:
		_attack_anim = max(0.0, _attack_anim - delta * 2.5)

	# ── Jambes : oscillation avant/arrière ──────────────────────────────────────
	var leg_angle := sin(_anim_time) * 0.45 if is_moving else 0.0
	if _pivot_left_leg:
		_pivot_left_leg.rotation.x  = leg_angle
	if _pivot_right_leg:
		_pivot_right_leg.rotation.x = -leg_angle

	# ── Bras : balancement opposé aux jambes ────────────────────────────────────
	var arm_swing := sin(_anim_time) * 0.30 if is_moving else 0.0

	if _pivot_left_arm:
		_pivot_left_arm.rotation.x = -arm_swing

	# Bras droit : balancement + soulèvement lors de l'attaque
	var raise := _attack_anim * -1.2   # -1.2 rad ≈ lever le bras vers l'avant/haut
	if _pivot_right_arm:
		_pivot_right_arm.rotation.x = arm_swing + raise

# ─── Comportements ─────────────────────────────────────────────────────────────
func _scan_for_player() -> void:
	if player == null:
		player = get_tree().get_first_node_in_group("player")
	if player == null:
		return
	if global_position.distance_to(player.global_position) <= detection_range \
			and _has_line_of_sight():
		state = State.CHASE

func _do_patrol() -> void:
	if patrol_points.is_empty():
		return
	var target := patrol_points[patrol_index]
	var dist   := global_position.distance_to(target)

	if dist < 1.5:
		patrol_index = (patrol_index + 1) % patrol_points.size()
		return

	var dir := (target - global_position).normalized()
	dir.y   = 0
	velocity.x = dir.x * walk_speed
	velocity.z = dir.z * walk_speed

func _do_chase() -> void:
	if player == null:
		state = State.PATROL
		return

	var dist := global_position.distance_to(player.global_position)

	if dist <= attack_range:
		state = State.ATTACK
		velocity.x = 0
		velocity.z = 0
		return

	var dir := (player.global_position - global_position).normalized()
	dir.y   = 0
	velocity.x = dir.x * chase_speed
	velocity.z = dir.z * chase_speed
	_look_toward(global_position + Vector3(dir.x, 0, dir.z))

func _do_attack() -> void:
	if player == null:
		state = State.PATROL
		return

	var dist := global_position.distance_to(player.global_position)
	if dist > attack_range * 1.5:
		state = State.CHASE
		return

	_look_toward(Vector3(player.global_position.x, global_position.y, player.global_position.z))

	velocity.x = 0
	velocity.z = 0

	if not _has_line_of_sight():
		state = State.CHASE
		return

	if can_attack:
		can_attack = false
		_fire_projectile()
		audio_player.play()
		attack_timer.start()

func _has_line_of_sight() -> bool:
	if player == null:
		return false
	var from  := global_position + Vector3(0, 1.4, 0)
	var to    := player.global_position + Vector3(0, 0.9, 0)
	var query := PhysicsRayQueryParameters3D.create(from, to)
	query.exclude = [get_rid()]
	var result := get_world_3d().direct_space_state.intersect_ray(query)
	return not result.is_empty() and (result["collider"] as Node).is_in_group("player")

func _fire_projectile() -> void:
	_attack_anim = 1.0   # déclenche l'animation de levée du bras

	var muzzle := global_position + Vector3(0, 1.2, 0)
	var target := player.global_position + Vector3(0, 0.9, 0)
	var dir    := (target - muzzle).normalized()

	muzzle += dir * 0.6

	var proj := preload("res://scripts/projectile.gd").new()
	proj.direction = dir
	proj.damage    = attack_damage
	get_tree().current_scene.add_child(proj)
	proj.global_position = muzzle

func _look_toward(target: Vector3) -> void:
	if target != global_position:
		look_at(target, Vector3.UP)

# ─── Dégâts & mort ─────────────────────────────────────────────────────────────
func take_damage(amount: float) -> void:
	if state == State.DEAD:
		return

	health -= amount

	if state == State.PATROL:
		if player == null:
			player = get_tree().get_first_node_in_group("player")
		state = State.CHASE

	if health <= 0.0:
		_die(false)

func zat_stun() -> void:
	if state == State.DEAD:
		return
	state      = State.STUNNED
	stun_timer = 3.0
	velocity   = Vector3.ZERO

func _die(disintegrate: bool) -> void:
	state    = State.DEAD
	velocity = Vector3.ZERO
	set_physics_process(false)
	set_process(false)
	GameManager.notify_enemy_killed()

	var tween := create_tween()
	if disintegrate:
		tween.tween_property(self, "scale", Vector3(0.01, 0.01, 0.01), 0.5)
	else:
		tween.tween_property(self, "rotation:z", PI / 2.0, 0.3)
		tween.tween_property(self, "scale", Vector3(0.0, 0.0, 0.0), 0.5)
	tween.tween_callback(queue_free)

# ─── Visuel Jaffa ──────────────────────────────────────────────────────────────
func _build_mesh() -> void:
	var armor := _mat(Color(0.10, 0.07, 0.06), 0.85, 0.35)
	var gold  := _mat(Color(0.72, 0.55, 0.08), 0.95, 0.15)
	var skin  := _mat(Color(0.52, 0.36, 0.22), 0.00, 0.85)
	var metal := _mat(Color(0.18, 0.16, 0.14), 0.80, 0.40)

	# ── Jambes avec pivots à la hanche ──────────────────────────────────────────
	_pivot_left_leg  = _make_pivot(Vector3(-0.12, 0.95, 0))
	_pivot_right_leg = _make_pivot(Vector3( 0.12, 0.95, 0))

	# Les jambes sont décalées vers le bas par rapport au pivot de hanche
	_cyl_on(_pivot_left_leg,  Vector3(0, -0.48, 0), 0.10, 0.95, armor)
	_cyl_on(_pivot_right_leg, Vector3(0, -0.48, 0), 0.10, 0.95, armor)

	# Genouillères sur les pivots
	_box_on(_pivot_left_leg,  Vector3(0, -0.40, 0.07), Vector3(0.13, 0.08, 0.07), gold)
	_box_on(_pivot_right_leg, Vector3(0, -0.40, 0.07), Vector3(0.13, 0.08, 0.07), gold)

	# ── Ceinture (fixe) ─────────────────────────────────────────────────────────
	_box(Vector3(0, 0.95, 0), Vector3(0.38, 0.09, 0.30), gold)

	# ── Torse (fixe) ────────────────────────────────────────────────────────────
	_cyl(Vector3(0, 1.18, 0), 0.20, 0.44, armor)
	_box(Vector3(0, 1.22, 0.13), Vector3(0.34, 0.28, 0.06), armor)   # plastron
	_box(Vector3(0, 1.28, 0.17), Vector3(0.10, 0.10, 0.04), gold)    # emblème

	# ── Bras avec pivots à l'épaule ─────────────────────────────────────────────
	_pivot_left_arm  = _make_pivot(Vector3(-0.25, 1.46, 0))
	_pivot_right_arm = _make_pivot(Vector3( 0.25, 1.46, 0))

	_cyl_on(_pivot_left_arm,  Vector3(0, -0.25, 0), 0.065, 0.44, armor)
	_cyl_on(_pivot_right_arm, Vector3(0, -0.25, 0), 0.065, 0.44, armor)

	# Épaulières sur les pivots
	_box_on(_pivot_left_arm,  Vector3(0,  0.00, 0), Vector3(0.15, 0.08, 0.22), gold)
	_box_on(_pivot_right_arm, Vector3(0,  0.00, 0), Vector3(0.15, 0.08, 0.22), gold)

	# ── Bâton de combat attaché au bras droit ───────────────────────────────────
	# Décalé en X pour tenir dans la main droite
	_cyl_on(_pivot_right_arm, Vector3(0.18, -0.55, 0), 0.022, 1.80, metal)   # hampe
	_sphere_on(_pivot_right_arm, Vector3(0.18,  0.35, 0), 0.060, gold)        # cellule basse
	_sphere_on(_pivot_right_arm, Vector3(0.18,  0.43, 0), 0.055, gold)        # cellule haute
	_box_on(_pivot_right_arm, Vector3(0.18,  0.52, 0), Vector3(0.035, 0.10, 0.035), gold)  # pointe

	# ── Cou + tête (fixes) ──────────────────────────────────────────────────────
	_cyl(Vector3(0, 1.60, 0), 0.072, 0.10, skin)
	_sphere(Vector3(0, 1.72, 0), 0.155, skin)

	# ── Casque cobra ─────────────────────────────────────────────────────────────
	_sphere(Vector3(0, 1.76, 0), 0.185, armor)
	_box(Vector3(0, 2.00, -0.04), Vector3(0.055, 0.22, 0.055), gold)
	_box(Vector3(-0.16, 1.91, -0.06), Vector3(0.055, 0.17, 0.04), gold)
	_box(Vector3( 0.16, 1.91, -0.06), Vector3(0.055, 0.17, 0.04), gold)

# ─── Helpers de construction ───────────────────────────────────────────────────
func _make_pivot(pos: Vector3) -> Node3D:
	var p := Node3D.new()
	p.position = pos
	add_child(p)
	return p

func _mat(color: Color, metallic: float, roughness: float) -> StandardMaterial3D:
	var m := StandardMaterial3D.new()
	m.albedo_color = color
	m.metallic     = metallic
	m.roughness    = roughness
	return m

# Helpers attachés au CharacterBody3D (positions absolues)
func _cyl(pos: Vector3, radius: float, height: float, mat: StandardMaterial3D) -> void:
	var mesh := CylinderMesh.new()
	mesh.top_radius    = radius
	mesh.bottom_radius = radius
	mesh.height        = height
	var mi := MeshInstance3D.new()
	mi.mesh              = mesh
	mi.material_override = mat
	mi.position          = pos
	add_child(mi)

func _box(pos: Vector3, size: Vector3, mat: StandardMaterial3D) -> void:
	var mesh := BoxMesh.new()
	mesh.size = size
	var mi := MeshInstance3D.new()
	mi.mesh              = mesh
	mi.material_override = mat
	mi.position          = pos
	add_child(mi)

func _sphere(pos: Vector3, radius: float, mat: StandardMaterial3D) -> void:
	var mesh := SphereMesh.new()
	mesh.radius = radius
	mesh.height = radius * 2.0
	var mi := MeshInstance3D.new()
	mi.mesh              = mesh
	mi.material_override = mat
	mi.position          = pos
	add_child(mi)

# Helpers attachés à un pivot (positions relatives au pivot)
func _cyl_on(pivot: Node3D, pos: Vector3, radius: float, height: float, mat: StandardMaterial3D) -> void:
	var mesh := CylinderMesh.new()
	mesh.top_radius    = radius
	mesh.bottom_radius = radius
	mesh.height        = height
	var mi := MeshInstance3D.new()
	mi.mesh              = mesh
	mi.material_override = mat
	mi.position          = pos
	pivot.add_child(mi)

func _box_on(pivot: Node3D, pos: Vector3, size: Vector3, mat: StandardMaterial3D) -> void:
	var mesh := BoxMesh.new()
	mesh.size = size
	var mi := MeshInstance3D.new()
	mi.mesh              = mesh
	mi.material_override = mat
	mi.position          = pos
	pivot.add_child(mi)

func _sphere_on(pivot: Node3D, pos: Vector3, radius: float, mat: StandardMaterial3D) -> void:
	var mesh := SphereMesh.new()
	mesh.radius = radius
	mesh.height = radius * 2.0
	var mi := MeshInstance3D.new()
	mi.mesh              = mesh
	mi.material_override = mat
	mi.position          = pos
	pivot.add_child(mi)
