-- name: GetShopByID :one
SELECT * FROM shop WHERE id = ?;