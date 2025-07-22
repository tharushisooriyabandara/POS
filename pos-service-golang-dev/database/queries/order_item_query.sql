-- name: CreateOrderItem :execlastid
INSERT INTO
    order_items (
        order_id,
        item_id,
        quantity,
        price_per_item,
        total,
        original_price,
        is_sale,
        discount_amount,
        status,
        created_at,
        updated_at,
        item_name,
        category_name,
        modifiers,
        tax,
        note
    )
VALUES (
        ?,
        ?,
        ?,
        ?,
        ?,
        ?,
        ?,
        ?,
        ?,
        ?,
        ?,
        ?,
        ?,
        ?,
        ?,
        ?
    );

-- name: GetAvailableOrderItems :many
SELECT *
FROM entity_delivery_platform
WHERE
    external_item_id IN (sqlc.slice (item_ids))
    AND delivery_platform_id = ?
    AND available = 1;