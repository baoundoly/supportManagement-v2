import * as signalR from '@microsoft/signalr'
import { useAuthStore } from '../store/authStore'

class SignalRService {
  private connection: signalR.HubConnection | null = null
  private listeners: Map<string, ((...args: any[]) => void)[]> = new Map()

  async connect() {
    if (this.connection?.state === signalR.HubConnectionState.Connected) return

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/support', {
        accessTokenFactory: () => useAuthStore.getState().accessToken ?? '',
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    // Re-register listeners after reconnect
    this.connection.onreconnected(() => {
      this.listeners.forEach((handlers, event) => {
        handlers.forEach(handler => {
          this.connection?.on(event, handler)
        })
      })
    })

    await this.connection.start()
  }

  async disconnect() {
    await this.connection?.stop()
    this.connection = null
  }

  on(event: string, handler: (...args: any[]) => void) {
    if (!this.listeners.has(event)) this.listeners.set(event, [])
    this.listeners.get(event)!.push(handler)
    this.connection?.on(event, handler)
  }

  off(event: string, handler: (...args: any[]) => void) {
    const handlers = this.listeners.get(event)
    if (handlers) {
      const index = handlers.indexOf(handler)
      if (index > -1) handlers.splice(index, 1)
    }
    this.connection?.off(event, handler)
  }

  async invoke(method: string, ...args: any[]) {
    await this.connection?.invoke(method, ...args)
  }

  get isConnected() {
    return this.connection?.state === signalR.HubConnectionState.Connected
  }
}

export const signalRService = new SignalRService()
