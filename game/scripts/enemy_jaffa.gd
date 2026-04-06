extends CharacterBody3D

# ─── États ─────────────────────────────────────────────────────────────────────
enum State { PATROL, CHASE, ATTACK, STUNNED, DEAD }

# ─── Paramètres ────────────────────────────────────────────────────────────────
@export var max_health: float     = 80.0
@export var walk_speed: float     = 2.5
@export var chase_speed: float    = 4.5
@export var detection_range: float = 18.0
@export var attack_range: float   = 7.0
@export var attack_damage: float  = 12.0
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

# ─── Références ────────────────────────────────────────────────────────────────
@onready var attack_timer: Timer = $AttackTimer

# ─── Initialisation ────────────────────────────────────────────────────────────
func _ready() -> void:
	health = max_health
	add_to_group("enemy")
	GameManager.register_enemy()

	attack_timer.wait_time = attack_cooldown
	attack_timer.one_shot  = true
	attack_timer.timeout.connect(func(): can_attack = true)

	# Points de patrouille autour du spawn
	var s := global_position
	patrol_points = [s, s + Vector3(4, 0, 0), s + Vector3(4, 0, 4), s + Vector3(0, 0, 4)]

	_build_mesh()

# ─── Boucle physique ───────────────────────────────────────────────────────────
func _physics_process(delta: float) -> void:
	if state == State.DEAD:
		return

	# Gravité
	if not is_on_floor():
		velocity.y -= GRAVITY * delta

	# Étourdissement
	if state == State.STUNNED:
		stun_timer -= delta
		velocity.x = move_toward(velocity.x, 0, 10.0)
		velocity.z = move_toward(velocity.z, 0, 10.0)
		if stun_timer <= 0.0:
			state = State.CHASE
		move_and_slide()
		return

	# Détection du joueur
	if state == State.PATROL:
		_scan_for_player()

	match state:
		State.PATROL: _do_patrol()
		State.CHASE:  _do_chase()
		State.ATTACK: _do_attack(delta)

	move_and_slide()

# ─── Comportements ─────────────────────────────────────────────────────────────
func _scan_for_player() -> void:
	if player == null:
		player = get_tree().get_first_node_in_group("player")
	if player == null:
		return
	if global_position.distance_to(player.global_position) <= detection_range:
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

	# Orientation vers le joueur
	var look_target := global_position + Vector3(dir.x, 0, dir.z)
	if look_target != global_position:
		look_at(look_target, Vector3.UP)

func _do_attack(_delta: float) -> void:
	if player == null:
		state = State.PATROL
		return

	var dist := global_position.distance_to(player.global_position)
	if dist > attack_range * 1.5:
		state = State.CHASE
		return

	# Orientation vers le joueur
	var look_target := Vector3(player.global_position.x, global_position.y, player.global_position.z)
	if look_target != global_position:
		look_at(look_target, Vector3.UP)

	velocity.x = 0
	velocity.z = 0

	if can_attack and player.has_method("take_damage"):
		can_attack = false
		player.take_damage(attack_damage)
		attack_timer.start()

# ─── Dégâts & mort ─────────────────────────────────────────────────────────────
func take_damage(amount: float) -> void:
	if state == State.DEAD:
		return

	health -= amount

	# Alerter le Jaffa s'il patrouillait
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
	GameManager.notify_enemy_killed()

	var tween := create_tween()
	if disintegrate:
		# Effet désintégration : teinte bleue puis disparition
		tween.tween_property($Body, "modulate", Color(0.3, 0.5, 1.0, 0.0), 0.6)
	else:
		# Chute et fondu
		tween.tween_property(self, "rotation:z", PI / 2.0, 0.3)
		tween.tween_property(self, "modulate", Color(1, 1, 1, 0), 0.8)
	tween.tween_callback(queue_free)

# ─── Visuel placeholder ────────────────────────────────────────────────────────
func _build_mesh() -> void:
	# Corps (capsule bleue/rouge = Jaffa)
	var body := CSGCylinder3D.new()
	body.name   = "Body"
	body.radius = 0.35
	body.height = 1.6
	body.position = Vector3(0, 0.8, 0)
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(0.15, 0.10, 0.08)  # Armure sombre Jaffa
	body.material = mat
	add_child(body)

	# Tête
	var head := CSGSphere3D.new()
	head.radius   = 0.28
	head.position = Vector3(0, 1.75, 0)
	var mat2 := StandardMaterial3D.new()
	mat2.albedo_color = Color(0.65, 0.50, 0.35)
	head.material = mat2
	add_child(head)

	# Détail : emblème Goa'uld (disque doré sur la poitrine)
	var emblem := CSGBox3D.new()
	emblem.size     = Vector3(0.15, 0.15, 0.05)
	emblem.position = Vector3(0, 1.0, 0.33)
	var mat3 := StandardMaterial3D.new()
	mat3.albedo_color = Color(0.80, 0.65, 0.10)
	mat3.metallic     = 0.9
	emblem.material   = mat3
	add_child(emblem)
