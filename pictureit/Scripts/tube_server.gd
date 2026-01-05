extends Node
@onready var tube_server: TubeClient = get_node("../TubeClient")
@export var session_id: String = ""

func _ready():
	tube_server.create_session()
	session_id = tube_server.session_id
	print("Server session created with ID: %s" % tube_server.session_id)
