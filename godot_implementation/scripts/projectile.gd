extends Node3D

var speed: float  = 18.0
var damage: float = 12.0
var direction: Vector3 = Vector3.FORWARD

const MAX_LIFETIME := 4.0
var _lifetime: float = 0.0

func _ready() -> void:
	_build_visual()

	var area := Area3D.new()
	var col  := CollisionShape3D.new()
	var shape := SphereShape3D.new()
	shape.radius = 0.18
	col.shape = shape
	area.add_child(col)
	area.body_entered.connect(_on_hit)
	add_child(area)

func _process(delta: float) -> void:
	_lifetime += delta
	if _lifetime >= MAX_LIFETIME:
		queue_free()
		return
	position += direction * speed * delta

func _on_hit(body: Node3D) -> void:
	if body.is_in_group("player"):
		body.take_damage(damage)
		queue_free()
	elif not body.is_in_group("enemy"):
		# Collision avec la géométrie du niveau
		queue_free()

func _build_visual() -> void:
	var sphere := CSGSphere3D.new()
	sphere.radius = 0.12
	var mat := StandardMaterial3D.new()
	mat.albedo_color              = Color(1.0, 0.55, 0.05)
	mat.emission_enabled          = true
	mat.emission                  = Color(1.0, 0.35, 0.0)
	mat.emission_energy_multiplier = 4.0
	sphere.material = mat
	add_child(sphere)
