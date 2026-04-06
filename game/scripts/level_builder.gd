extends Node3D

# Construit tout le niveau en CSG — aucun asset 3D externe requis.
# Palette de couleurs : ambiance base militaire Goa'uld sur Abydos (désert + métal)

const COL_SAND   := Color(0.76, 0.60, 0.42)   # Sol sableux
const COL_WALL   := Color(0.52, 0.42, 0.28)   # Murs pierre/grès
const COL_METAL  := Color(0.28, 0.28, 0.32)   # Structures métalliques
const COL_GOLD   := Color(0.70, 0.55, 0.10)   # Détails Goa'uld dorés
const COL_CRATE  := Color(0.40, 0.32, 0.20)   # Caisses

func _ready() -> void:
	_build_floor()
	_build_exterior_walls()
	_build_interior_layout()
	_build_cover()
	_build_decorations()

# ─── Sol ───────────────────────────────────────────────────────────────────────
func _build_floor() -> void:
	_box(Vector3(0, -0.5, 0), Vector3(52, 1, 64), COL_SAND)

# ─── Murs extérieurs ───────────────────────────────────────────────────────────
func _build_exterior_walls() -> void:
	var h := 5.0   # hauteur des murs
	var t := 1.0   # épaisseur

	# Mur Nord (ouverture pour la Porte)
	_box(Vector3(-13, h/2, -32), Vector3(26, h, t), COL_WALL)    # gauche
	_box(Vector3(13,  h/2, -32), Vector3(26, h, t), COL_WALL)    # droite
	_box(Vector3(0,   h,   -32), Vector3(10, 1, t), COL_WALL)    # linteau

	# Mur Sud (entrée joueur)
	_box(Vector3(-14, h/2, 32), Vector3(24, h, t), COL_WALL)
	_box(Vector3(14,  h/2, 32), Vector3(24, h, t), COL_WALL)
	_box(Vector3(0,   h,   32), Vector3(8,  1, t), COL_WALL)

	# Murs Est / Ouest
	_box(Vector3(26,  h/2, 0), Vector3(t, h, 64), COL_WALL)
	_box(Vector3(-26, h/2, 0), Vector3(t, h, 64), COL_WALL)

# ─── Disposition intérieure ────────────────────────────────────────────────────
func _build_interior_layout() -> void:
	# Mur de séparation central (couloir central libre)
	_box(Vector3(-14, 2.0,  2), Vector3(24, 4, 0.8), COL_WALL)
	_box(Vector3( 14, 2.0,  2), Vector3(24, 4, 0.8), COL_WALL)

	# Couloirs latéraux
	_box(Vector3(-22, 2.0, -8),  Vector3(0.8, 4, 20), COL_WALL)
	_box(Vector3( 22, 2.0, -8),  Vector3(0.8, 4, 20), COL_WALL)

	# Salle de commandement (autour de la Porte)
	_box(Vector3(0,   2.0, -18), Vector3(18, 4, 0.8), COL_METAL)
	_box(Vector3(-9,  2.0, -24), Vector3(0.8, 4, 12), COL_METAL)
	_box(Vector3( 9,  2.0, -24), Vector3(0.8, 4, 12), COL_METAL)

	# Piliers de la grande salle
	for px in [-8.0, 8.0]:
		for pz in [8.0, -4.0]:
			_box(Vector3(px, 2.5, pz), Vector3(1.2, 5, 1.2), COL_METAL)
			# Capitel doré
			_box(Vector3(px, 5.1, pz), Vector3(1.6, 0.3, 1.6), COL_GOLD)

# ─── Éléments de couverture ────────────────────────────────────────────────────
func _build_cover() -> void:
	# Zone de spawn joueur
	_box(Vector3(-4, 0.5, 22),  Vector3(2, 1, 2), COL_CRATE)
	_box(Vector3( 4, 0.5, 22),  Vector3(2, 1, 2), COL_CRATE)
	_box(Vector3(-4, 1.5, 22),  Vector3(2, 1, 2), COL_CRATE)   # caisses empilées

	# Barricades zone centrale
	_box(Vector3(-3, 0.5,  10), Vector3(4, 1, 0.6), COL_CRATE)
	_box(Vector3( 3, 0.5,  10), Vector3(4, 1, 0.6), COL_CRATE)
	_box(Vector3( 0, 0.5,  -2), Vector3(3, 1, 0.6), COL_METAL)

	# Consoles de commandement (covers devant la Porte)
	_box(Vector3(-4, 0.8, -12), Vector3(3.5, 1.6, 1.2), COL_METAL)
	_box(Vector3( 4, 0.8, -12), Vector3(3.5, 1.6, 1.2), COL_METAL)

	# Détails lumineux sur les consoles
	_box(Vector3(-4, 1.65, -11.4), Vector3(3.0, 0.1, 0.1), Color(0.2, 0.8, 0.4))
	_box(Vector3( 4, 1.65, -11.4), Vector3(3.0, 0.1, 0.1), Color(0.2, 0.8, 0.4))

# ─── Décorations Goa'uld ───────────────────────────────────────────────────────
func _build_decorations() -> void:
	# Sarcophage décoratif (à gauche)
	_box(Vector3(-18, 0.6, -5), Vector3(2.5, 1.2, 1.0), COL_GOLD)

	# Urnes / totems
	for tx in [-15.0, 15.0]:
		var urn := CSGCylinder3D.new()
		urn.radius   = 0.3
		urn.height   = 1.8
		urn.position = Vector3(tx, 0.9, 18)
		var mat := StandardMaterial3D.new()
		mat.albedo_color = COL_GOLD
		mat.metallic     = 0.7
		urn.material = mat
		add_child(urn)

	# Rampe d'accès à la zone Porte
	var ramp := CSGBox3D.new()
	ramp.size          = Vector3(10, 0.6, 5)
	ramp.position      = Vector3(0, 0.0, -16)
	ramp.rotation.x    = deg_to_rad(-7)
	ramp.use_collision = true
	var rmat := StandardMaterial3D.new()
	rmat.albedo_color = COL_WALL
	ramp.material = rmat
	add_child(ramp)

# ─── Utilitaire ────────────────────────────────────────────────────────────────
func _box(pos: Vector3, size: Vector3, color: Color) -> CSGBox3D:
	var b   := CSGBox3D.new()
	b.size          = size
	b.position      = pos
	b.use_collision = true
	var mat    := StandardMaterial3D.new()
	mat.albedo_color = color
	b.material = mat
	add_child(b)
	return b
