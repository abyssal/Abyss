package com.abyssaldev.abyss.gateway

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.util.Loggable
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import net.dv8tion.jda.api.events.ReadyEvent
import net.dv8tion.jda.api.hooks.ListenerAdapter

class GatewayReadyListenerAdapter: ListenerAdapter(), Loggable {
    override fun onReady(event: ReadyEvent) {
        logger.info("Received Discord READY signal. Connected as ${event.jda.selfUser}")
        GlobalScope.launch {
            AbyssEngine.instance.interactions.registerAllInteractions()
        }
    }
}