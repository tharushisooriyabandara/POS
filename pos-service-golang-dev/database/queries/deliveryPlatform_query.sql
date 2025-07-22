-- name: GetDeliveryPlatform :one
SELECT * FROM delivery_platform WHERE outlet_id = ? AND webshop_brand_id = ? AND platform_id = ?;