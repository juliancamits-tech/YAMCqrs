DomainEventCommand and DomainEventHandler are just for be called in the API

DomainEventHandler create a "Domain event" using IEventPublisher

DomainEventPublishEvent is the DTO with the information of the event

DomainEventSubscribeEvent is the clone of DomainEventPublishEvent that recibe the message 

DomainEventSubscribeHanlder is the logic that process the event

With this focus tomorrow DomainEventSubscribeEvent and DomainEventSubscribeHanlder can be moved to another project and with small change in DomainEventPublishEvent the event should be dispatched to X service bus broker