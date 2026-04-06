extends Node3D

# La Porte des Étoiles — condition de victoire
# Le joueur doit éliminer tous les Jaffa avant de pouvoir passer.

var portal_active: bool = false

@onready var area: Area3D       = $Area3D
@onready var ring: CSGTorus3D   = $Ring
@onready var portal: CSGBox3D   = $Portal

func _ready() -> void:
	if portal and not portal.material:
		var mat := StandardMaterial3D.new()
		mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
		portal.material = mat
	area.body_entered.connect(_on_body_entered)
	GameManager.enemy_killed.connect(_on_enemy_killed)
	_set_portal_state(false)

func _on_enemy_killed(remaining: int) -> void:
	if remaining == 0:
		_set_portal_state(true)

func _set_portal_state(active: bool) -> void:
	portal_active = active
	if portal and portal.material:
		var mat := portal.material as StandardMaterial3D
		if active:
			mat.albedo_color     = Color(0.2, 0.6, 1.0, 0.85)
			mat.emission_enabled = true
			mat.emission         = Color(0.1, 0.4, 0.9)
		else:
			mat.albedo_color     = Color(0.1, 0.1, 0.15, 0.4)
			mat.emission_enabled = false

func _on_body_entered(body: Node3D) -> void:
	if not body.is_in_group("player"):
		return

	if portal_active:
		GameManager.player_won()
	else:
		# Feedback : la porte est inactive (flash scale)
		var tween := create_tween()
		tween.tween_property(ring, "scale", Vector3(1.12, 1.12, 1.12), 0.1)
		tween.tween_property(ring, "scale", Vector3(1.0, 1.0, 1.0), 0.3)

func _process(_delta: float) -> void:
	# Animation de rotation de l'anneau quand la porte est active
	if portal_active:
		ring.rotation.z += 0.01
