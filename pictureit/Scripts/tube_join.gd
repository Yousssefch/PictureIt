extends Node

@onready var tube_client: TubeClient = get_node("../TubeClient")
@export var session_id = ""

func _ready():
	tube_client.join_session(session_id)

func leave_session():
	tube_client.leave_session()
