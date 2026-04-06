extends WeaponBase

func _ready() -> void:
	weapon_name  = "P90"
	damage       = 18.0
	fire_rate    = 0.08     # ~750 coups/min
	max_ammo     = 50
	reserve_ammo = 200
	reload_time  = 2.2
	is_automatic = true
	super._ready()

func _build_mesh() -> void:
	# Corps du P90 (bloc gris métallique)
	var body := CSGBox3D.new()
	body.size = Vector3(0.06, 0.08, 0.35)
	body.position = Vector3(0, 0, -0.05)
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(0.25, 0.25, 0.28)
	body.material = mat
	add_child(body)

	# Chargeur (rectangle en dessous)
	var mag := CSGBox3D.new()
	mag.size = Vector3(0.05, 0.12, 0.1)
	mag.position = Vector3(0, -0.08, -0.02)
	var mat2 := StandardMaterial3D.new()
	mat2.albedo_color = Color(0.18, 0.18, 0.20)
	mag.material = mat2
	add_child(mag)

	# Canon
	var barrel := CSGCylinder3D.new()
	barrel.radius = 0.012
	barrel.height = 0.25
	barrel.rotation.x = PI / 2.0
	barrel.position = Vector3(0, 0.01, -0.22)
	var mat3 := StandardMaterial3D.new()
	mat3.albedo_color = Color(0.15, 0.15, 0.15)
	barrel.material = mat3
	add_child(barrel)
