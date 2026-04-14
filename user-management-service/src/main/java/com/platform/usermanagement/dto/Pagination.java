package com.platform.usermanagement.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

/**
 * Pagination metadata for paginated responses.
 */
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class Pagination {

    private long totalItems;
    private int totalPages;
    private int currentPage;
    private int itemsPerPage;
}
