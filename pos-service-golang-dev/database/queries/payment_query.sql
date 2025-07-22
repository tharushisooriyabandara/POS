-- name: CreateOrderPayments :exec
INSERT INTO
    `dg_pos_payments` (
        order_id,
        date_time,
        amount,
        cash,
        balance,
        transaction_id,
        refund_id,
        payment_method_id,
        status,
        is_refund,
        created_at,
        updated_at
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
        ?
    );