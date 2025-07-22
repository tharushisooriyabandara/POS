package types

type GetUsersResponse struct {
	ID        uint64 `json:"id"`
	FirstName string `json:"first_name"`
	LastName  string `json:"last_name"`
	Email     string `json:"email"`
	RoleId    string `json:"role_id"`
	Role      string `json:"role"`
}

/*
	types for authentication
*/

// LoginResponse is the response format for the login endpoint
type LoginResponse struct {
	AccessToken  string `json:"accessToken"`
	RefreshToken string `json:"refreshToken"`
}

/***********************************************/
