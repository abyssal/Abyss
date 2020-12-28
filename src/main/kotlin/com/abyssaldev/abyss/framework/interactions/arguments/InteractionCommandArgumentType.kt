package com.abyssaldev.abyss.framework.interactions.arguments

enum class InteractionCommandArgumentType(val raw: Int) {
    Subcommand(1),
    SubcommandGroup(2),
    String(3),
    Integer(4),
    Boolean(5),
    User(6),
    Channel(7),
    Role(8)
}