create table if not exists outbox_message
(
    id
        bigint
        generated
            by
            default as
            identity,
    destination
        text,
    key
        bytea,
    value
        bytea
        not
            null,
    created_at
        timestamp
        not
            null
);